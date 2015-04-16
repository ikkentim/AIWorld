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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
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
        private Model _placeHolderModel;
        private List<Plane> _terrainTiles;
        private BasicEffect _basicEffect;
        private Texture2D _grass;
        private float _modelRotation;
        private Texture2D _roadTexture;
        private Road _mainRoad;
        private Vehicle _tracingVehicle;

        public Simulation()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            _aspectRatio = _graphics.GraphicsDevice.Viewport.AspectRatio;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _grass = Content.Load<Texture2D>(@"textures/grass");
            _roadTexture = Content.Load<Texture2D>(@"textures/road");
//            _placeHolderModel = Content.Load<Model>("pawnred");
            _placeHolderModel = Content.Load<Model>("models/car");
            _basicEffect = new BasicEffect(GraphicsDevice);

            //Create terrain
            _terrainTiles = new List<Plane>();
            for (int x = -5; x <= 5; x++)
                for (int y = -5; y <= 5; y++)
                    _terrainTiles.Add(new Plane(GraphicsDevice, new Vector3(x*4, -0.01f, y*4), 4, PlaneRotation.None,
                        _grass));

            // Create road
            _mainRoad = new Road(GraphicsDevice, _roadTexture, new[]
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

            var vehicle = new Vehicle(new Vector3(0, 0, 1), Content, _mainRoad);

            _tracingVehicle = vehicle;
            _world.Entities.Insert(vehicle);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();

            if (kb.IsKeyDown(Keys.Down))
                _cameraDistance -= (float) gameTime.ElapsedGameTime.TotalSeconds;

            if (kb.IsKeyDown(Keys.Up))
                _cameraDistance += (float) gameTime.ElapsedGameTime.TotalSeconds;


            if (kb.IsKeyDown(Keys.Left))
                _cameraRotation -= (float) gameTime.ElapsedGameTime.TotalSeconds;

            if (kb.IsKeyDown(Keys.Right))
                _cameraRotation += (float) gameTime.ElapsedGameTime.TotalSeconds;


            _modelRotation += (float) gameTime.ElapsedGameTime.TotalMilliseconds*
                              MathHelper.ToRadians(0.1f);

            _world.Update(gameTime);

            if (_tracingVehicle != null)
                _cameraTarget = _tracingVehicle.Position;

            base.Update(gameTime);
        }


        private void Line(Vector3 a, Vector3 b, Color c)
        {
            var vertices = new[] {new VertexPositionColor(a, c), new VertexPositionColor(b, c)};
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        protected override void Draw(GameTime gameTime)
        {
            Vector3 realCameraTarget = _cameraTarget + new Vector3(0, 0.5f, 0);
            Vector3 cameraPosition = realCameraTarget +
                                     new Vector3((float) Math.Cos(_cameraRotation), 1, (float) Math.Sin(_cameraRotation))*
                                     _cameraDistance;

            Matrix view = Matrix.CreateLookAt(cameraPosition, realCameraTarget, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), _aspectRatio, 0.1f,
                10000.0f);

            _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // basicEffect for line drawing...
//            _basicEffect.VertexColorEnabled = true;
//            _basicEffect.World = Matrix.Identity;
//            _basicEffect.View = view;
//            _basicEffect.Projection = projection;
//
//            _basicEffect.CurrentTechnique.Passes[0].Apply();

            // Draw vehicles
            _world.Render(GraphicsDevice, view, projection, gameTime);
            // /

            // Draw tiles

            foreach (Plane t in _terrainTiles)
            {
                t.Render(GraphicsDevice, Matrix.Identity, view, projection);
            }

            foreach (Plane t in _mainRoad.Planes)
            {
                t.Render(GraphicsDevice, Matrix.Identity, view, projection);
            }
            // /

            base.Draw(gameTime);
        }
    }

    class Road
    {
        public Vector3[] Nodes { get; private set; }

        public Plane[] Planes { get; private set; }

        public Road(GraphicsDevice graphicsDevice, Texture2D texture, Vector3[] nodes)
        {
            Nodes = nodes;
            Planes = RoadPlanesGenerator.Generate(graphicsDevice, texture, nodes).ToArray();
        }
    }
}