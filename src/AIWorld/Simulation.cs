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
using AIWorld.Entities;
using AIWorld.Services;
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
        private Road _mainRoad;
        private List<Plane> _terrainTiles;
        private Vehicle _tracingVehicle;
        private SoundEffect _ambientEffect;
        private bool _isMiddleButtonDown;
        private int _lastScroll;
        private float _scrollVelocity;
        private float _unprocessedScrollDelta;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Simulation" /> class.
        /// </summary>
        public Simulation()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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

            var ambient = _ambientEffect.CreateInstance();
            ambient.IsLooped = true;
            ambient.Volume = 0.015f;
            ambient.Play();

            //Create terrain
            _terrainTiles = new List<Plane>();
            for (int x = -5; x <= 5; x++)
                for (int y = -5; y <= 5; y++)
                    _terrainTiles.Add(new Plane(GraphicsDevice, new Vector3(x*4, -0.01f, y*4), 4, PlaneRotation.None,
                        _grass));

            // Create road
            _mainRoad = new Road(GraphicsDevice, Content.Load<Texture2D>(@"textures/road"), new[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(2, 0, 0),
                new Vector3(3, 0, 1),
                new Vector3(3, 0, 2),
                new Vector3(3, 0, 3),
                new Vector3(3, 0, 4),
                new Vector3(3, 0, 5),
                new Vector3(3, 0, 6),
                new Vector3(3, 0, 7),
                new Vector3(3, 0, 8),
                new Vector3(2, 0, 9),
                new Vector3(1, 0, 9),
                new Vector3(0, 0, 9),
                new Vector3(-1, 0, 9),
                new Vector3(-2, 0, 8),
                new Vector3(-2, 0, 7),
                new Vector3(-2, 0, 6),
                new Vector3(-2, 0, 5),
                new Vector3(-2, 0, 4),
                new Vector3(-2, 0, 3),
                new Vector3(-3, 0, 2),
                new Vector3(-4, 0, 2),
                new Vector3(-5, 0, 2),
                new Vector3(-6, 0, 2),
                new Vector3(-7, 0, 2),
            });

            //Create vehicle
            var vehicle = new Vehicle(new Vector3(1, 0, 1), this, _mainRoad);

            _tracingVehicle = vehicle;
            _gameWorldService.Add(vehicle);
            _gameWorldService.Add(new Vehicle(new Vector3(5, 0, 5), this, _mainRoad));
            _gameWorldService.Add(new Vehicle(new Vector3(10, 0, 10), this, _mainRoad));
            _gameWorldService.Add(new Vehicle(new Vector3(15, 0, 15), this, _mainRoad));

            _gameWorldService.Add(new House(new Vector3(2.8f, 0, 2.0f), this, 0));
            _gameWorldService.Add(new House(new Vector3(3.2f, 0, 4.0f), this, 0));
            _gameWorldService.Add(new House(new Vector3(2.8f, 0, 6.0f), this, 0));
            _gameWorldService.Add(new House(new Vector3(3.2f, 0, 8.0f), this, 0));
        }

        /// <summary>
        ///     Unloads the content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
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

            Vector3 realCameraTarget = _cameraTarget + new Vector3(0, CameraTargetOffset, 0);
            Vector3 cameraPosition = realCameraTarget +
                                     new Vector3((float) Math.Cos(_cameraRotation),
                                         Use45DegreeCamera
                                             ? 1
                                             : _cameraDistance/3 - MinZoom + CameraTargetOffset + CameraHeightOffset,
                                         (float) Math.Sin(_cameraRotation))*
                                     _cameraDistance;

            _cameraService.Update(cameraPosition, realCameraTarget, _aspectRatio);

            if (_tracingVehicle != null)
                _cameraTarget = _tracingVehicle.Position;
        }

        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw tiles

            foreach (Plane t in _terrainTiles)
                t.Render(GraphicsDevice, Matrix.Identity, _cameraService.View, _cameraService.Projection);

            foreach (Plane t in _mainRoad.Planes)
                t.Render(GraphicsDevice, Matrix.Identity, _cameraService.View, _cameraService.Projection);

            // /

            base.Draw(gameTime);
        }
    }
}