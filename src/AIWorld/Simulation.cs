// AIWorld
// Copyright 2015 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using AIWorld.Entities;
using AIWorld.Events;
using AIWorld.Planes;
using AIWorld.Scripting;
using AIWorld.Services;
using AMXWrapper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AIWorld
{
    /// <summary>
    ///     This is the main type for your game
    /// </summary>
    public class Simulation : Game, IScripted
    {
        private const float ScrollMaxSpeed = 0.3f;
        private const float ScrollMultiplier = 0.01f;
        private const float ScrollMinDelta = 0.05f;
        private const float ScrollModifier = 2.5f;
        private const float MouseRotationModifier = 0.005f;
        private readonly GraphicsDeviceManager _graphics;
        private readonly string _scriptName;
        private Color _backgroundColor;
        private ICameraService _cameraService;
        private IConsoleService _consoleService;
        private IGameWorldService _gameWorldService;
        private IDrawingService _drawingService;
        private IParticleService _particleService;
        private KeyboardState _lastKeyboardState;
        private MouseState _lastMouseState;
        private int _lastScroll;
        private AMXPublic _onKeyStateChanged;
        private AMXPublic _onMouseClick;
        private float _scrollVelocity;
        private float _unprocessedScrollDelta;
        private readonly List<SoundEffectInstance> _soundEffectInstances = new List<SoundEffectInstance>(); 
        private readonly List<SoundEffect> _soundEffects = new List<SoundEffect>(); 
        /// <summary>
        ///     Initializes a new instance of the <see cref="Simulation" /> class.
        /// </summary>
        public Simulation(string scriptName)
        {
            // Store the script we want to start for later use.
            if ((_scriptName = scriptName) == null)
                throw new ArgumentNullException("scriptName");
   
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        public ScriptBox Script { get; private set; }
        public event EventHandler<MouseClickEventArgs> MouseClick;
        public event EventHandler<KeyStateEventArgs> KeyStateChanged;

        private void LoadSimulation()
        {
            // Reset background color to a default value.
            _backgroundColor = Color.CornflowerBlue;

            // Register various services.
            Services.AddService(typeof (IConsoleService), _consoleService = new ConsoleService(this));
            Services.AddService(typeof (ICameraService), _cameraService = new CameraService(this));
            Services.AddService(typeof(IGameWorldService), _gameWorldService = new GameWorldService(this, _cameraService));
            Services.AddService(typeof(IDrawingService), _drawingService = new DrawingService(this, _cameraService));
            Services.AddService(typeof(IParticleService), _particleService = new ParticleService(this));

            Components.Add(_consoleService);
            Components.Add(_cameraService);
            Components.Add(_gameWorldService);
            Components.Add(_drawingService);
            Components.Add(_particleService);

            // Set up the script and let it handle further setup.
            try
            {
                Script = new ScriptBox("main", _scriptName);
                Script.Register(this, _gameWorldService, _consoleService, _drawingService);

                _onMouseClick = Script.FindPublic("OnMouseClick");
                _onKeyStateChanged = Script.FindPublic("OnKeyStateChanged");

                Script.ExecuteMain();
            }
            catch (Exception e)
            {
                _consoleService.WriteLine(Color.Red, e);
                Script = null;
            }
        }

        private void UnloadSimulation()
        {
            Debug.WriteLine("UnloadSimulation");

            if (Script != null)
                Script.Dispose();
            Script = null;

            // Unload all compolents and services.
            foreach(var c in Components.OfType<IDisposable>())
                c.Dispose();
            Components.Clear();

            foreach (var instance in _soundEffectInstances)
            {
                instance.Stop();
                instance.Dispose();
            }

            _soundEffects.Clear();
            _soundEffectInstances.Clear();
      
            _gameWorldService = null;
            _cameraService = null;
            _consoleService = null;

            Services.RemoveService(typeof (IConsoleService));
            Services.RemoveService(typeof (ICameraService));
            Services.RemoveService(typeof (IGameWorldService));
            Services.RemoveService(typeof (IDrawingService));

            GC.Collect();
        }

        /// <summary>
        ///     Loads the content.
        /// </summary>
        protected override void LoadContent()
        {
            LoadSimulation();
        }

        /// <summary>
        ///     Unloads the content.
        /// </summary>
        protected override void UnloadContent()
        {
            UnloadSimulation();
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            // First things first: if F5 is pressed reboot the simulation.
            if (IsActive && keyboardState.IsKeyDown(Keys.F5) && !_lastKeyboardState.IsKeyDown(Keys.F5))
            {
                _lastMouseState = mouseState;
                _lastKeyboardState = keyboardState;

                UnloadSimulation();
                LoadSimulation();
                return;
            }

            // Prevent user input while not in focus or no script is running.
            if (!IsActive || Script == null)
            {
                _lastMouseState = mouseState;
                _lastKeyboardState = keyboardState;
                return;
            }

            #region Process camera manipulation input

            float deltaCameraRotation = 0;
            float deltaCameraZoom = 0;

            // If middle or right button is pressed, hide the mouse button and track the x-axis (rotation) movements
            if (mouseState.MiddleButton == ButtonState.Pressed || mouseState.RightButton == ButtonState.Pressed)
            {
                // If this is the first update in which the button is pressed, recenter the mouse and
                // wait for the next update, in which we actually update the mouse rotation
                if (_lastMouseState.MiddleButton == ButtonState.Pressed ||
                    _lastMouseState.RightButton == ButtonState.Pressed)
                {
                    // Add rotation delta based on mouse movement
                    deltaCameraRotation += ((float)mouseState.X - Window.ClientBounds.Width/2)*MouseRotationModifier;

                    // Recenter mouse.
                    Mouse.SetPosition(
                        Window.ClientBounds.Width/2,
                        Window.ClientBounds.Height/2);
                }
                else
                {
                    // Hide and center mouse.
                    IsMouseVisible = false;
                    Mouse.SetPosition(
                        Window.ClientBounds.Width/2,
                        Window.ClientBounds.Height/2);

                    // TODO: It might be better only to recenter the mouse when you start dragging and when you stop relocate the mouse to were you started.
                }
            }
            else if (_lastMouseState.MiddleButton == ButtonState.Pressed ||
                     _lastMouseState.RightButton == ButtonState.Pressed)
            {
                // Button is released; show mouse again.
                IsMouseVisible = true;
            }

            // Calculate the delta scroll value.
            var scroll = mouseState.ScrollWheelValue;
            var deltaScroll = scroll - _lastScroll;
            _lastScroll = scroll;

            // Add the delta scroll to the unprocessed scroll value. 
            _unprocessedScrollDelta -= deltaScroll;

            // If there is some reasonable amount of unprocessed scroll, handle it.
            if (Math.Abs(_unprocessedScrollDelta) > ScrollMinDelta)
            {
                _scrollVelocity = MathHelper.Clamp(_unprocessedScrollDelta*ScrollMultiplier, -ScrollMaxSpeed,
                    ScrollMaxSpeed);
           
                _unprocessedScrollDelta -= _scrollVelocity/ScrollMultiplier;

                deltaCameraZoom += (_scrollVelocity*_cameraService.Zoom/
                                   (CameraService.MaxZoom - CameraService.MinZoom + 1))*ScrollModifier;
            }

            Vector3 acceleration = Vector3.Zero;
            if (keyboardState.IsKeyDown(Keys.Left)) acceleration += Vector3.Backward;
            if (keyboardState.IsKeyDown(Keys.Right)) acceleration += Vector3.Forward;
            if (keyboardState.IsKeyDown(Keys.Up)) acceleration += Vector3.Left;
            if (keyboardState.IsKeyDown(Keys.Down)) acceleration += Vector3.Right;

            _cameraService.AddVelocity(acceleration*(float) gameTime.ElapsedGameTime.TotalSeconds);
            _cameraService.Move(deltaCameraRotation, deltaCameraZoom);

            #endregion

            #region Handle keyboard state

            if (keyboardState != _lastKeyboardState)
                OnKeyStateChanged(new KeyStateEventArgs(keyboardState.GetPressedKeys(),
                    _lastKeyboardState.GetPressedKeys()));

            #endregion

            #region Handle clicking

            // Check which buttons were pressed.
            var leftMouseButtonPressed = mouseState.LeftButton == ButtonState.Pressed &&
                                         _lastMouseState.LeftButton == ButtonState.Released;
            var middleMouseButtonPressed = mouseState.MiddleButton == ButtonState.Pressed &&
                                           _lastMouseState.MiddleButton == ButtonState.Released;
            var rightMouseButtonPressed = mouseState.RightButton == ButtonState.Pressed &&
                                          _lastMouseState.RightButton == ButtonState.Released;

            // If any button was pressed, calculate click position.
            if (leftMouseButtonPressed || middleMouseButtonPressed || rightMouseButtonPressed)
            {
                // Create a ray based on the clicked position.
                var nearPoint = GraphicsDevice.Viewport.Unproject(new Vector3(mouseState.Position.ToVector2(), 0),
                    _cameraService.Projection, _cameraService.View, Matrix.Identity);

                var farPoint = GraphicsDevice.Viewport.Unproject(new Vector3(mouseState.Position.ToVector2(), 1),
                    _cameraService.Projection, _cameraService.View, Matrix.Identity);

                var ray = new Ray(nearPoint, Vector3.Normalize(farPoint - nearPoint));

                // Test where the ray hits the ground.
                var groundDistance = ray.Intersects(new Plane(Vector3.Up, 0));
                if (groundDistance != null)
                {
                    // If the ray hits the ground, look for nearby entities and find which one was clicked.

                    Vector3 clickPostion = nearPoint + ray.Direction*groundDistance.Value;

                    var clickedEntity =
                        _gameWorldService.Entities.Query(new AABB(clickPostion, Vector3.One*(Entity.MaxSize/2)))
                            .FirstOrDefault(e => ray.Intersects(new BoundingSphere(e.Position, e.Size)) != null);

                    // Fire click events for every pressed button. If an entity is clicked and it handles the call,
                    // don't call the general OnMouseClick event.
                    foreach (var args in Enumerable.Range(1, 3)
                        .Where(
                            n =>
                                (n == 1 && leftMouseButtonPressed) || (n == 2 && middleMouseButtonPressed) ||
                                (n == 3 && rightMouseButtonPressed))
                        .Select(button => new MouseClickEventArgs(button, clickPostion))
                        .Where(args => clickedEntity == null || clickedEntity.OnClicked(args) == false))
                        OnMouseClick(args);
                }
            }

            #endregion

            // Remember last status
            _lastMouseState = mouseState;
            _lastKeyboardState = keyboardState;
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clear the background with the specified color.
            _graphics.GraphicsDevice.Clear(_backgroundColor);

            base.Draw(gameTime);
        }

        [ScriptingFunction]
        public void AddQuadPlane(float x, float y, float z, float size, int rotation, string texture)
        {
            // Create the quad plane.
            _gameWorldService.Add(new QuadPlane(this, new Vector3(x, y, z), size, (PlaneRotation) rotation,
                Content.Load<Texture2D>(texture)));
        }

        [ScriptingFunction]
        public int AddAgent(AMXArgumentList arguments)
        {
            if (arguments.Length < 3) return -1;

            var scriptname = arguments[0].AsString();
            var x = arguments[1].AsFloat();
            var y = arguments[2].AsFloat();

            // Create the entity and return the id.
            Agent agent = new Agent(this, scriptname, new Vector3(x, 0, y));

            _gameWorldService.Add(agent);

            if (arguments.Length > 4)
                DefaultFunctions.SetVariables(agent, arguments, 3);

            agent.Start();

            return agent.Id;
        }

        [ScriptingFunction]
        public int AddGameObject(string name, float size, float x, float y, float sx, float sy, float sz,float rx, float ry, float rz, float tx, float ty, float tz,  string meshes)
        {
            // Create the entity and return the id.
            var obj = new WorldObject(this, name, size, new Vector3(x, 0, y), new Vector3(rx, ry, rz),
                new Vector3(tx, ty, tz), new Vector3(sx, sy, sz),
                meshes.Split(',').Select(v => v.Trim()).Where(v => v.Length > 0));

            _gameWorldService.Add(obj);

            return obj.Id;
        }

        [ScriptingFunction]
        public bool AddRoad(string key, IntPtr arrayPointer, int count)
        {
            // Verify argument count.
            if (count%2 != 0) count--;
            if (count < 4) return false;

            // Build a nodes list from the arguments.
            var nodes = new List<Vector3>();
            for (var i = 0; i < count/2; i++)
            {
                var x = Cell.FromIntPtr(IntPtr.Add(arrayPointer, (i*2 + 0)*Marshal.SizeOf(typeof (Cell))));
                var y = Cell.FromIntPtr(IntPtr.Add(arrayPointer, (i*2 + 1)*Marshal.SizeOf(typeof (Cell))));

                nodes.Add(new Vector3(x.AsFloat(), 0, y.AsFloat()));
            }

            // Generate the road and add the nodes to the specified graph.
            Road.GenerateRoad(this, _gameWorldService[key], nodes.ToArray());

            return true;
        }

        [ScriptingFunction]
        public void SetBackgroundColor(int colorCode)
        {
            var a = (colorCode >> 8*0) & 0xFF;
            var r = (colorCode >> 8*3) & 0xFF;
            var g = (colorCode >> 8*2) & 0xFF;
            var b = (colorCode >> 8*1) & 0xFF;

            _backgroundColor =
                new Color(new Vector4((float) r/byte.MaxValue, (float) g/byte.MaxValue, (float) b/byte.MaxValue,
                    (float) a/byte.MaxValue));
        }

        [ScriptingFunction]
        public void PlayAmbience(string sound, bool isLooped, float volume, float pitch, float pan)
        {
            var ambientEffect = _soundEffects.FirstOrDefault(e => e.Name == sound);
            if (ambientEffect == null)
            {
                ambientEffect = Content.Load<SoundEffect>(sound);
                _soundEffects.Add(ambientEffect);
            }

            var ambient = ambientEffect.CreateInstance();
            ambient.IsLooped = isLooped;
            ambient.Volume = volume;
            ambient.Pitch = pitch;
            ambient.Pan = pan;
            ambient.Play();

            _soundEffectInstances.Add(ambient);
        }

        [ScriptingFunction]
        public int StopAmbience()
        {
            foreach (var instance in _soundEffectInstances)
            {
                instance.Stop(true);
                instance.Dispose();
            }

            var result = _soundEffectInstances.Count;
            _soundEffectInstances.Clear();

            return result;
        }

        /// <summary>
        ///     Raises the <see cref="E:MouseClick" /> event.
        /// </summary>
        /// <param name="e">The <see cref="MouseClickEventArgs" /> instance containing the event data.</param>
        protected virtual void OnMouseClick(MouseClickEventArgs e)
        {
            if (_onMouseClick != null && Script != null)
            {
                // Push the arguments and call the callback.
                Script.Push(e.Position.Z);
                Script.Push(e.Position.X);
                Script.Push(e.Button);

                if (_onMouseClick.Execute() == 1) return;
                // If main handled the click, don't call the MouseClick event.
            }

            if (MouseClick != null)
                MouseClick(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="E:KeyStateChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="KeyStateEventArgs" /> instance containing the event data.</param>
        protected virtual void OnKeyStateChanged(KeyStateEventArgs e)
        {
            if (_onKeyStateChanged != null && Script != null)
            {
                // Allocate an array for new keys and old keys in the abstract machine.
                var newKeys = Script.Allot(e.NewKeys.Length + 1);
                var oldKeys = Script.Allot(e.OldKeys.Length + 1);

                // Copy the keys to the machine.
                for (var i = 0; i < e.NewKeys.Length; i++)
                    (newKeys + i).Set((int) e.NewKeys[i]);

                for (var i = 0; i < e.OldKeys.Length; i++)
                    (oldKeys + i).Set((int) e.OldKeys[i]);

                (newKeys + e.NewKeys.Length).Set((int) Keys.None);
                (oldKeys + e.OldKeys.Length).Set((int) Keys.None);

                // Push the arguments and call the callback.
                Script.Push(oldKeys);
                Script.Push(newKeys);

                var result = _onKeyStateChanged.Execute();

                // Release the arrays.
                Script.Release(newKeys);
                Script.Release(oldKeys);

                if (result == 1) return;
            }

            if (KeyStateChanged != null)
                KeyStateChanged(this, e);
        }
    }
}