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
using System.Runtime.InteropServices;
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
        private readonly GraphicsDeviceManager _graphics;
        private readonly GameWorld _world = new GameWorld();

        private float _aspectRatio;
        private float _cameraDistance = 3;
        private float _cameraRotation;
        private Vector3 _cameraTarget = new Vector3(0.0f, 0.0f, 0.0f);
        private List<Plane> _terrainTiles;
        private Texture2D _grass;
        private Road _mainRoad;
        private Vehicle _tracingVehicle;
        private SoundEffect ambientEffect;
        private BasicEffect _basicEffect;
        private ICameraService cameraService;
        /// <summary>
        /// Initializes a new instance of the <see cref="Simulation"/> class.
        /// </summary>
        public Simulation()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            _aspectRatio = _graphics.GraphicsDevice.Viewport.AspectRatio;

            Services.AddService(typeof (ICameraService), cameraService = new CameraService());

            base.Initialize();
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        protected override void LoadContent()
        {
            _grass = Content.Load<Texture2D>(@"textures/grass");
            ambientEffect = Content.Load<SoundEffect>(@"sounds/ambient");
            SoundEffect engine = Content.Load<SoundEffect>(@"sounds/engine");
            _basicEffect = new BasicEffect(GraphicsDevice);

            var house01 = Content.Load<Model>(@"models/house01");

            var inst = ambientEffect.CreateInstance();
            inst.IsLooped = true;
            inst.Volume = 0.015f;
            inst.Play();

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
            });

            //Create vehicle
            var vehicle = new Vehicle(new Vector3(1, 0, 1), GraphicsDevice, engine, Content, _mainRoad);

            _tracingVehicle = vehicle;
            _world.Entities.Insert(vehicle);
            _world.Entities.Insert(new Vehicle(new Vector3(5, 0, 5), GraphicsDevice, engine, Content, _mainRoad));
            _world.Entities.Insert(new Vehicle(new Vector3(10, 0, 10), GraphicsDevice, engine, Content, _mainRoad));
            _world.Entities.Insert(new Vehicle(new Vector3(15, 0, 15), GraphicsDevice, engine, Content, _mainRoad));

            _world.Entities.Insert(new House(new Vector3(2.8f, 0, 2.0f), house01, 0));
            _world.Entities.Insert(new House(new Vector3(3.2f, 0, 4.0f), house01, 0));
            _world.Entities.Insert(new House(new Vector3(2.8f, 0, 6.0f), house01, 0));
            _world.Entities.Insert(new House(new Vector3(3.2f, 0, 8.0f), house01, 0));
        }

        /// <summary>
        /// Unloads the content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private int lastScroll;
        private float unprocessedScrollDelta;
        private float scrollVelocity;
        private bool isMiddleButtonDown;
        private const float minZoom = 1;
        private const float maxZoom = 20;

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState kb = Keyboard.GetState();

            var ms = Mouse.GetState();

            var scroll = ms.ScrollWheelValue;
            var deltaScroll = scroll - lastScroll;
            lastScroll = scroll;

            unprocessedScrollDelta -= deltaScroll * 0.0075f;

            if (Math.Abs(unprocessedScrollDelta) > 0.5)
            {
                const float maxScrollSpeed = 1f;
                const float decelerationTweaker = 5f;
                const float cameraSpeedModifier = 2.5f;
                scrollVelocity += Math.Min(unprocessedScrollDelta/decelerationTweaker, maxScrollSpeed) - scrollVelocity;

                unprocessedScrollDelta -= scrollVelocity;

                _cameraDistance += scrollVelocity*_cameraDistance/(maxZoom - minZoom + 1)*cameraSpeedModifier;
            }

            if (ms.MiddleButton == ButtonState.Pressed)
            {
                if (isMiddleButtonDown)
                {
                    //_cameraRotation
                    var mx = ms.X;

                    _cameraRotation += (mx - Window.ClientBounds.Width / 2) / 100f;

                    Mouse.SetPosition(
                        Window.ClientBounds.Width/2,
                        Window.ClientBounds.Height/2);
                }
                else
                {
                    isMiddleButtonDown = true;
                    Mouse.SetPosition(
                        Window.ClientBounds.Width / 2,
                        Window.ClientBounds.Height / 2);
                }
            }
            else
            {
                isMiddleButtonDown = false;
            }

            if (kb.IsKeyDown(Keys.Down))
                _cameraDistance -= deltaTime;

            if (kb.IsKeyDown(Keys.Up))
                _cameraDistance += deltaTime;


            if (kb.IsKeyDown(Keys.Left))
                _cameraRotation -= deltaTime;

            if (kb.IsKeyDown(Keys.Right))
                _cameraRotation += deltaTime;

            if (_cameraDistance < minZoom)
            {
                _cameraDistance = minZoom;
                unprocessedScrollDelta = 0;
            }
            if (_cameraDistance > maxZoom)
            {
                _cameraDistance = maxZoom;
                unprocessedScrollDelta = 0;
            }

            const float cameraTargetOffset = 0.2f;
            const float cameraHeightOffset = 0.75f;
            const bool use45DegreeCamera = false;

            Vector3 realCameraTarget = _cameraTarget + new Vector3(0, cameraTargetOffset, 0);
            Vector3 cameraPosition = realCameraTarget +
                                     new Vector3((float)Math.Cos(_cameraRotation),
                                         use45DegreeCamera ? 1 : _cameraDistance / 3 - minZoom + cameraTargetOffset + cameraHeightOffset,
                                         (float)Math.Sin(_cameraRotation)) *
                                     _cameraDistance;

            cameraService.Update(cameraPosition, realCameraTarget, _aspectRatio);

            _world.Update(cameraService.View, cameraService.Projection, gameTime);

            if (_tracingVehicle != null)
                _cameraTarget = _tracingVehicle.Position;
        }


        private void Line(Vector3 a, Vector3 b, Color c)
        {
            var vertices = new[] {new VertexPositionColor(a, c), new VertexPositionColor(b, c)};
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // basicEffect for line drawing...
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.World = cameraService.World;
            _basicEffect.View = cameraService.View;
            _basicEffect.Projection = cameraService.Projection;

            _basicEffect.CurrentTechnique.Passes[0].Apply();

            foreach (var entity in _world.Entities)
            {
                var pos = entity.Position;
                var sizeh = new Vector3(entity.Size, 0, 0);
                var sizev = new Vector3(0, 0, entity.Size);

                var posses = new[]
                {
                    pos + sizeh,
                    pos + sizev,
                    pos - sizeh,
                    pos - sizev
                };

                foreach (var p in posses)
                    Line(p, p + Vector3.Up, Color.Red);
            }

            // Draw vehicles
            _world.Render(GraphicsDevice, cameraService.View, cameraService.Projection, gameTime);
            // /

            // Draw tiles

            foreach (Plane t in _terrainTiles)
                t.Render(GraphicsDevice, Matrix.Identity, cameraService.View, cameraService.Projection);
            
            foreach (Plane t in _mainRoad.Planes)
                t.Render(GraphicsDevice, Matrix.Identity, cameraService.View, cameraService.Projection);
            
            // /

            base.Draw(gameTime);
        }
    }

    interface ICameraService
    {
        Matrix World { get; }
        Matrix View { get; }
        Matrix Projection { get; }

        void Update(Vector3 cameraPosition, Vector3 cameraTargetPosition, float aspectRatio);
    }

    class CameraService : ICameraService
    {
        public CameraService()
        {
            World = Matrix.Identity;
            View = Matrix.Identity;
            Projection = Matrix.Identity;
        }

        #region Implementation of ICameraService

        public Matrix World { get; private set; }
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public void Update(Vector3 cameraPosition, Vector3 cameraTargetPosition, float aspectRatio)
        {
            View = Matrix.CreateLookAt(cameraPosition, cameraTargetPosition, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 0.1f,
                10000.0f);
        }

        #endregion
    }
}