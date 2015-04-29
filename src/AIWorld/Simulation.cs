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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using AIWorld.Entities;
using AIWorld.Helpers;
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
    public class Simulation : Game
    {
        private const float MinZoom = 1;
        private const float MaxZoom = 20;
        private const float MaxScrollSpeed = 1f;
        private const float DecelerationTweaker = 5f;
        private const float CameraSpeedModifier = 2.5f;
        private const float CameraTargetOffset = 0.2f;
        private const float CameraHeightOffset = 0.75f;
        private const bool Use45DegreeCamera = false;

        private readonly GraphicsDeviceManager _graphics;
        private float _aspectRatio;
        private float _cameraDistance = 3;
        private float _cameraRotation;
        private ICameraService _cameraService;
        private Vector3 _cameraTarget = new Vector3(0.0f, 0.0f, 0.0f);
        private GameWorldService _gameWorldService;
        private Texture2D _grass;
        private Entity _tracingEntity;
        private SoundEffect _ambientEffect;
        private bool _isMiddleButtonDown;
        private int _lastScroll;
        private float _scrollVelocity;
        private float _unprocessedScrollDelta;
        private BasicEffect _basicEffect;

        private ScriptBox _script;
        /// <summary>
        ///     Initializes a new instance of the <see cref="Simulation" /> class.
        /// </summary>
        public Simulation()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _script = new ScriptBox("main", "main");
            _script.Register<string,float,float,float,float>(AddGameObject);
            _script.Register<IntPtr,int>(AddRoad);
            _script.Register<string,float,float>(AddAgent);
        }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            _aspectRatio = _graphics.GraphicsDevice.Viewport.AspectRatio;

            Services.AddService(typeof (ICameraService), _cameraService = new CameraService());
            Services.AddService(typeof (IGameWorldService), _gameWorldService = new GameWorldService(this));

            base.Initialize();
        }

        /// <summary>
        ///     Loads the content.
        /// </summary>
        protected override void LoadContent()
        {
            _grass = Content.Load<Texture2D>(@"textures/grass");
            _ambientEffect = Content.Load<SoundEffect>(@"sounds/ambient");
            _basicEffect = new BasicEffect(GraphicsDevice);
            var ambient = _ambientEffect.CreateInstance();
            ambient.IsLooped = true;
            ambient.Volume = 0.015f;
            ambient.Play();

            //Create terrain
            for (int x = -5; x <= 5; x++)
                for (int y = -5; y <= 5; y++)
                    _gameWorldService.Add(new QuadPlane(this, new Vector3(x*4, -0.01f, y*4), 4, PlaneRotation.None,
                        _grass));

            _script.ExecuteMain();

            //Create vehicles
//            _gameWorldService.Add(_tracingEntity = new Vehicle(new Vector3(1, 0, 1), this));
//            _gameWorldService.Add(new Vehicle(new Vector3(8, 0, 5), this));
//            _gameWorldService.Add(new Vehicle(new Vector3(10, 0, 10), this));
//            _gameWorldService.Add(new Vehicle(new Vector3(15, 0, 15), this));

        }

        #region scripting natives

        private int AddAgent(string scriptname, float x, float y)
        {
            _gameWorldService.Add(new Agent(this, scriptname, new Vector3(x, 0, y)));
            return 1;
        }

        private int AddRoad(IntPtr arrayPointer, int count)
        {
            if (count % 2 != 0)
                count--;
            if (count < 4)
                return 0;

            var nodes = new List<Vector3>();
            for (var i = 0; i < count / 2; i++)
            {
                var x = Cell.FromIntPtr(IntPtr.Add(arrayPointer, (i*2 + 0)*Marshal.SizeOf(typeof (Cell))));
                var y = Cell.FromIntPtr(IntPtr.Add(arrayPointer, (i*2 + 1)*Marshal.SizeOf(typeof (Cell))));

                nodes.Add(new Vector3(x.AsFloat(), 0, y.AsFloat()));
            }

            Road.GenerateRoad(this, nodes.ToArray());

            return 1;
        }

        private int AddGameObject(string name, float size, float x, float y, float angle)
        {
            _gameWorldService.Add(new WorldObject(this, name, size, new Vector3(x, 0, y), angle, false));
            return 1;
        }

        #endregion

        /// <summary>
        ///     Unloads the content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private bool _leftclick;
        protected override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            base.Update(gameTime);

            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            var scroll = mouseState.ScrollWheelValue;
            var deltaScroll = scroll - _lastScroll;
            _lastScroll = scroll;

            _unprocessedScrollDelta -= deltaScroll*0.0075f;

            if (Math.Abs(_unprocessedScrollDelta) > 0.5)
            {
                _scrollVelocity += Math.Min(_unprocessedScrollDelta/DecelerationTweaker, MaxScrollSpeed) - _scrollVelocity;

                _unprocessedScrollDelta -= _scrollVelocity;

                _cameraDistance += _scrollVelocity*_cameraDistance/(MaxZoom - MinZoom + 1)*CameraSpeedModifier;
            }

            if (mouseState.MiddleButton == ButtonState.Pressed || mouseState.RightButton == ButtonState.Pressed)
            {
                if (_isMiddleButtonDown)
                {
                    //_cameraRotation
                    int mx = mouseState.X;

                    _cameraRotation += (mx - Window.ClientBounds.Width/2)/100f;

                    Mouse.SetPosition(
                        Window.ClientBounds.Width/2,
                        Window.ClientBounds.Height/2);
                }
                else
                {
                    _isMiddleButtonDown = true;
                    Mouse.SetPosition(
                        Window.ClientBounds.Width/2,
                        Window.ClientBounds.Height/2);
                }
            }
            else
            {
                _isMiddleButtonDown = false;
            }

            if (keyboardState.IsKeyDown(Keys.Down))
                _cameraDistance -= deltaTime;

            if (keyboardState.IsKeyDown(Keys.Up))
                _cameraDistance += deltaTime;


            if (keyboardState.IsKeyDown(Keys.Left))
                _cameraRotation -= deltaTime;

            if (keyboardState.IsKeyDown(Keys.Right))
                _cameraRotation += deltaTime;

            if (_cameraDistance < MinZoom)
            {
                _cameraDistance = MinZoom;
                _unprocessedScrollDelta = 0;
            }
            if (_cameraDistance > MaxZoom)
            {
                _cameraDistance = MaxZoom;
                _unprocessedScrollDelta = 0;
            }

            var realCameraTarget = _cameraTarget + new Vector3(0, CameraTargetOffset, 0);
            var cameraPosition = realCameraTarget +
                                     new Vector3((float)Math.Cos(_cameraRotation),
                                         Use45DegreeCamera
                                             ? 1
                                             : _cameraDistance / 3 - MinZoom + CameraTargetOffset + CameraHeightOffset,
                                         (float)Math.Sin(_cameraRotation)) *
                                     _cameraDistance;

            _cameraService.Update(cameraPosition, realCameraTarget, _aspectRatio);
            if (mouseState.LeftButton == ButtonState.Released)
            {
                _leftclick = false;
            }
            if (mouseState.LeftButton == ButtonState.Pressed && !_leftclick)
            {
                _leftclick = true;

                var nearsource = new Vector3(mouseState.Position.ToVector2(), 0f);
                var farsource = new Vector3(mouseState.Position.ToVector2(), 1f);

                var nearPoint = GraphicsDevice.Viewport.Unproject(nearsource,
                    _cameraService.Projection, _cameraService.View, Matrix.Identity);

                var farPoint = GraphicsDevice.Viewport.Unproject(farsource,
                    _cameraService.Projection, _cameraService.View, Matrix.Identity);

                var ray = new Ray(nearPoint, Vector3.Normalize(farPoint - nearPoint));

                var ground = new Plane(Vector3.Up, 0);

                var groundDistance = ray.Intersects(ground);

                if (groundDistance != null)
                {
                    var groundPos = nearPoint + ray.Direction*groundDistance.Value;

                    var rect = new AABB(groundPos, Vector3.One * 10);
                    var clicker =
                        _gameWorldService.Entities.Query(rect)
                            .OfType<Entity>()
                            .FirstOrDefault(e => ray.Intersects(new BoundingSphere(e.Position, e.Size)) != null);

                    if (clicker != null)
                    {
                        _tracingEntity = clicker;
                    }
                }
            }

            if (_tracingEntity != null)
                _cameraTarget = _tracingEntity.Position;
        }

//        private void Line(Vector3 a, Vector3 b, Color c)
//        {
//            var vertices = new[] { new VertexPositionColor(a, c), new VertexPositionColor(b, c) };
//            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
//        }

        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

//            _basicEffect.VertexColorEnabled = true;
//            _basicEffect.World = Matrix.Identity;
//            _basicEffect.View = _cameraService.View;
//            _basicEffect.Projection = _cameraService.Projection;
//
//            _basicEffect.CurrentTechnique.Passes[0].Apply();

            base.Draw(gameTime);
        }
    }
}