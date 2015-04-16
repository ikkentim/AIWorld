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
        private readonly GameWorld world = new GameWorld();

        private float _aspectRatio;
        private float _cameraDistance = 3;
        private float _cameraRotation;
        private Vector3 _cameraTarget = new Vector3(0.0f, 0.0f, 0.0f);
        private Model _placeHolderModel;
        private IEnumerable<Plane> _roadTiles;
        private Plane[] _terrainTiles;
        private BasicEffect basicEffect;
        private Texture2D grass;
        private float modelRotation;
        private Texture2D roadTexture;

        private Road mainRoad;

        private Vehicle tracingVehicle;

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
            grass = Content.Load<Texture2D>(@"textures/grass");
            roadTexture = Content.Load<Texture2D>(@"textures/road");
            _placeHolderModel = Content.Load<Model>("pawnred");

            basicEffect = new BasicEffect(GraphicsDevice);

            //Create terrain
            _terrainTiles = new Plane[4*4];
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    _terrainTiles[y*4 + x] = new Plane(GraphicsDevice, new Vector3(x*4, -1, y*4), 4, PlaneRotation.None,
                        grass);

            // Create road
            mainRoad = new Road(GraphicsDevice, roadTexture, new[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(2, 0, 0),
                new Vector3(3, 0, 1),
                new Vector3(3, 0, 2),
                new Vector3(3, 0, 3),
            });

            //Create vehicle

            var vehicle = new Vehicle(new Vector3(0, 0, 1), mainRoad);

            tracingVehicle = vehicle;
            world.Entities.Insert(vehicle);
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


            modelRotation += (float) gameTime.ElapsedGameTime.TotalMilliseconds*
                             MathHelper.ToRadians(0.1f);

            world.Update(gameTime);

            if (tracingVehicle != null)
                _cameraTarget = tracingVehicle.Position;

            base.Update(gameTime);
        }


        private void Line(Vector3 a, Vector3 b, Color c)
        {
            var vertices = new[] { new VertexPositionColor(a, c), new VertexPositionColor(b, c) };
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

            // temp model
            var transforms = new Matrix[_placeHolderModel.Bones.Count];
            _placeHolderModel.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in _placeHolderModel.Meshes)
            {
                Vector3 pos = Vector3.Zero;
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index]*Matrix.CreateRotationY(modelRotation)*
                                   Matrix.CreateTranslation(pos);
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
            // /


            // basicEffect for line drawing...
            basicEffect.VertexColorEnabled = true;
            basicEffect.World = Matrix.Identity;
            basicEffect.View = view;
            basicEffect.Projection = projection;

            basicEffect.CurrentTechnique.Passes[0].Apply();

            // Draw vehicles
            world.Render(GraphicsDevice, gameTime);
            // /

            // Draw tiles

            foreach (Plane t in _terrainTiles)
            {
                t.Render(GraphicsDevice, Matrix.Identity, view, projection);
            }

            foreach (Plane t in mainRoad.Planes)
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