﻿// AIWorld
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using AIWorld.Entities;
using AIWorld.Helpers;
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
        private const float ScrollMaxSpeed = 1f;
        private const float ScrollMultiplier = 0.1f;
        private const float ScrollModifier = 2.5f;
        private readonly GraphicsDeviceManager _graphics;
        private readonly string _scriptName;
        private SoundEffect _ambientEffect;
        private Color _backgroundColor;
        private ICameraService _cameraService;
        private IConsoleService _consoleService;
        private IGameWorldService _gameWorldService;
        private KeyboardState _lastKeyboardState;
        private MouseState _lastMouseState;
        private int _lastScroll;
        private AMXPublic _onKeyStateChanged;
        private AMXPublic _onMouseClick;
        private float _scrollVelocity;
        private float _unprocessedScrollDelta;
        private Vector3 cameraVelocity = Vector3.Zero;
    

        /// <summary>
        ///     Initializes a new instance of the <see cref="Simulation" /> class.
        /// </summary>
        public Simulation(string scriptName)
        {
            if (scriptName == null) throw new ArgumentNullException("scriptName");
            _scriptName = scriptName;

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        public ScriptBox Script { get; private set; }
        public event EventHandler<MouseClickEventArgs> MouseClick;
        public event EventHandler<KeyStateEventArgs> KeyStateChanged;

        private void LoadSimulation()
        {
            _backgroundColor = Color.CornflowerBlue;

            Services.AddService(typeof (IConsoleService), _consoleService = new ConsoleService(this));
            Services.AddService(typeof (ICameraService), _cameraService = new CameraService(this));
            Services.AddService(typeof (IGameWorldService), _gameWorldService = new GameWorldService(this));

            Components.Add(_consoleService);
            Components.Add(_cameraService);
            Components.Add(_gameWorldService);

            try
            {
                Script = new ScriptBox("main", _scriptName);
                Script.Register(this, _gameWorldService, _consoleService);

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
            Components.Clear();

            Services.RemoveService(typeof (IConsoleService));
            Services.RemoveService(typeof (ICameraService));
            Services.RemoveService(typeof (IGameWorldService));
        }

        /// <summary>
        ///     Loads the content.
        /// </summary>
        protected override void LoadContent()
        {
            _ambientEffect = Content.Load<SoundEffect>(@"sounds/ambient");
            var ambient = _ambientEffect.CreateInstance();
            ambient.IsLooped = true;
            ambient.Volume = 0.015f;
            ambient.Play();

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

            if (IsActive && keyboardState.IsKeyDown(Keys.F5) && !_lastKeyboardState.IsKeyDown(Keys.F5))
            {
                _lastMouseState = mouseState;
                _lastKeyboardState = keyboardState;

                UnloadSimulation();
                LoadSimulation();
                return;
            }

            // Prevent keyboard input while not in focus
            if (!IsActive || Script == null) return;

            #region Update camera

            float deltaCameraRotation = 0;
            float deltaCameraZoom = 0;

            if (mouseState.MiddleButton == ButtonState.Pressed || mouseState.RightButton == ButtonState.Pressed)
            {
                if (_lastMouseState.MiddleButton == ButtonState.Pressed ||
                    _lastMouseState.RightButton == ButtonState.Pressed)
                {
                    deltaCameraRotation += (mouseState.X - Window.ClientBounds.Width/2)/100f;

                    Mouse.SetPosition(
                        Window.ClientBounds.Width/2,
                        Window.ClientBounds.Height/2);
                }
                else
                {
                    IsMouseVisible = false;
                    Mouse.SetPosition(
                        Window.ClientBounds.Width/2,
                        Window.ClientBounds.Height/2);
                }
            }
            else if (_lastMouseState.MiddleButton == ButtonState.Pressed ||
                     _lastMouseState.RightButton == ButtonState.Pressed)
            {
                IsMouseVisible = true;
            }

            var scroll = mouseState.ScrollWheelValue;
            var deltaScroll = scroll - _lastScroll;
            _lastScroll = scroll;

            _unprocessedScrollDelta -= deltaScroll*0.01f;

            if (Math.Abs(_unprocessedScrollDelta) > 0.5)
            {
                _scrollVelocity = Math.Min(_unprocessedScrollDelta*ScrollMultiplier, ScrollMaxSpeed);
                _unprocessedScrollDelta -= _scrollVelocity;

                deltaCameraZoom += _scrollVelocity*_cameraService.Zoom/
                                   (CameraService.MaxZoom - CameraService.MinZoom + 1)*ScrollModifier;
            }

            Vector3 acceleration = Vector3.Zero;
            if (keyboardState.IsKeyDown(Keys.Left)) acceleration += Vector3.Backward;
            if (keyboardState.IsKeyDown(Keys.Right)) acceleration += Vector3.Forward;
            if (keyboardState.IsKeyDown(Keys.Up)) acceleration += Vector3.Left;
            if (keyboardState.IsKeyDown(Keys.Down)) acceleration += Vector3.Right;

            _cameraService.AddVelocity(acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds);
            _cameraService.Move(deltaCameraRotation, deltaCameraZoom);

            #endregion

            #region Keyboard keys

            if (keyboardState != _lastKeyboardState)
                OnKeyStateChanged(new KeyStateEventArgs(keyboardState.GetPressedKeys(),
                    _lastKeyboardState.GetPressedKeys()));

            #endregion

            #region Mouse buttons

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
                var nearPoint = GraphicsDevice.Viewport.Unproject(new Vector3(mouseState.Position.ToVector2(), 0),
                    _cameraService.Projection, _cameraService.View, Matrix.Identity);

                var farPoint = GraphicsDevice.Viewport.Unproject(new Vector3(mouseState.Position.ToVector2(), 1),
                    _cameraService.Projection, _cameraService.View, Matrix.Identity);

                var ray = new Ray(nearPoint, Vector3.Normalize(farPoint - nearPoint));

                var groundDistance = ray.Intersects(new Plane(Vector3.Up, 0));
                if (groundDistance != null)
                {
                    Vector3 clickPostion = nearPoint + ray.Direction*groundDistance.Value;

                    var clickedEntity =
                        _gameWorldService.Entities.Query(new AABB(clickPostion, Vector3.One*(Entity.MaxSize/2)))
                            .FirstOrDefault(e => ray.Intersects(new BoundingSphere(e.Position, e.Size)) != null);

                    if (leftMouseButtonPressed)
                    {
                        var args = new MouseClickEventArgs(1, clickPostion);
                        if (clickedEntity == null || clickedEntity.OnClicked(args) == false)
                            OnMouseClick(args);
                    }
                    if (middleMouseButtonPressed)
                    {
                        var args = new MouseClickEventArgs(2, clickPostion);
                        if (clickedEntity == null || clickedEntity.OnClicked(args) == false)
                            OnMouseClick(args);
                    }
                    if (rightMouseButtonPressed)
                    {
                        var args = new MouseClickEventArgs(3, clickPostion);
                        if (clickedEntity == null || clickedEntity.OnClicked(args) == false)
                            OnMouseClick(args);
                    }
                }
            }

            #endregion

            // Remember last status
            _lastMouseState = mouseState;
            _lastKeyboardState = keyboardState;
        }

        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(_backgroundColor);

            base.Draw(gameTime);
        }

        [ScriptingFunction]
        public void AddQuadPlane(float x, float y, float z, float size, int rotation, string texture)
        {
            _gameWorldService.Add(new QuadPlane(this, new Vector3(x, y, z), size, (PlaneRotation) rotation,
                Content.Load<Texture2D>(texture)));
        }

        [ScriptingFunction]
        public int AddAgent(string scriptname, float x, float y)
        {
            Agent agent = new Agent(this, scriptname, new Vector3(x, 0, y));
            _gameWorldService.Add(agent);
            agent.Initialize();

            return agent.Id;
        }

        [ScriptingFunction]
        public int AddGameObject(string name, float size, float x, float y, float angle)
        {
            var obj = new WorldObject(this, name, size, new Vector3(x, 0, y), angle, false);
            _gameWorldService.Add(obj);

            return obj.Id;
        }

        [ScriptingFunction]
        public bool AddRoad(string key, IntPtr arrayPointer, int count)
        {
            if (count%2 != 0) count--;
            if (count < 4) return false;

            var nodes = new List<Vector3>();
            for (var i = 0; i < count/2; i++)
            {
                var x = Cell.FromIntPtr(IntPtr.Add(arrayPointer, (i*2 + 0)*Marshal.SizeOf(typeof (Cell))));
                var y = Cell.FromIntPtr(IntPtr.Add(arrayPointer, (i*2 + 1)*Marshal.SizeOf(typeof (Cell))));

                nodes.Add(new Vector3(x.AsFloat(), 0, y.AsFloat()));
            }

            Road.GenerateRoad(this, _gameWorldService[key], nodes.ToArray());

            return true;
        }

        /// <summary>
        ///     Raises the <see cref="E:MouseClick" /> event.
        /// </summary>
        /// <param name="e">The <see cref="MouseClickEventArgs" /> instance containing the event data.</param>
        protected virtual void OnMouseClick(MouseClickEventArgs e)
        {
            if (_onMouseClick != null)
            {
                Script.Push(e.Position.Z);
                Script.Push(e.Position.X);
                Script.Push(e.Button);

                if (_onMouseClick.Execute() == 1) return;
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
            if (_onKeyStateChanged != null)
            {
                var newKeys = Script.Allot(e.NewKeys.Length + 1);
                var oldKeys = Script.Allot(e.NewKeys.Length + 1);

                for (var i = 0; i < e.NewKeys.Length; i++)
                    (newKeys + i).Set((int) e.NewKeys[i]);

                for (var i = 0; i < e.OldKeys.Length; i++)
                    (oldKeys + i).Set((int) e.OldKeys[i]);

                (newKeys + e.NewKeys.Length).Set((int) Keys.None);
                (oldKeys + e.OldKeys.Length).Set((int) Keys.None);

                Script.Push(oldKeys);
                Script.Push(newKeys);

                var result = _onKeyStateChanged.Execute();

                Script.Release(newKeys);
                Script.Release(oldKeys);

                if (result == 1) return;
            }

            if (KeyStateChanged != null)
                KeyStateChanged(this, e);
        }
    }
}