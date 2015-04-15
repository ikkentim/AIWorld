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
        private readonly Vehicle vehicle = new Vehicle(new Vector3(0, 0, 1));
        private readonly GameWorld world = new GameWorld();

        private float _aspectRatio;
        private float _cameraDistance = 3;
        private float _cameraRotation;
        private Vector3 _cameraTarget = new Vector3(0.0f, 0.0f, 0.0f);
        private Model _placeHolderModel;
        private TerrainTile[] _terrainTiles;
        private BasicEffect basicEffect;
        private Texture2D grass;
        private float modelRotation;
        private BasicEffect quadEffect;


        private Vehicle tracingVehicle;

        public Simulation()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            var ab = new AABB(new Vector3(5, -5, -5), new Vector3(5, 5, 5));
            var vec = new Vector3(1.00000048f, 0, -0.00000041713255f);
            bool res = ab.ContainsPoint(vec);
        }

        protected override void Initialize()
        {
            _aspectRatio = _graphics.GraphicsDevice.Viewport.AspectRatio;

            basicEffect = new BasicEffect(GraphicsDevice);

            _terrainTiles = new TerrainTile[4*4];
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    _terrainTiles[y*4 + x] = new TerrainTile(new Vector3(x*TerrainTile.Size, 0, y*TerrainTile.Size));

            tracingVehicle = vehicle;
            world.Entities.Insert(vehicle);

            for (int x = -4; x <= 4; x++)
                for (int y = -4; y <= 4; y++)
                    if (!(x == 0 && y == 1))
                        world.Entities.Insert(new Vehicle(new Vector3(x, 0, y)));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            grass = Content.Load<Texture2D>(@"textures/grass");
            _placeHolderModel = Content.Load<Model>("pawnred");

            quadEffect = new BasicEffect(GraphicsDevice);
            quadEffect.EnableDefaultLighting();
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
            {
                _cameraTarget = tracingVehicle.Position;
            }

            base.Update(gameTime);
        }

        private void Line(Vector3 a, Vector3 b)
        {
            var vertices = new[] {new VertexPositionColor(a, Color.Black), new VertexPositionColor(b, Color.Black)};
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


            basicEffect.VertexColorEnabled = true;
            basicEffect.World = Matrix.Identity;
            basicEffect.View = view;
            basicEffect.Projection = projection;

            basicEffect.CurrentTechnique.Passes[0].Apply();

            // Draw tile lines
            foreach (TerrainTile t in _terrainTiles)
            {
                Line(t.Position, t.Position + new Vector3(0, 10, 0));
            }
            // /

            // Draw vehicles
            world.Render(GraphicsDevice, gameTime);

            // /

            // Draw tiles
            quadEffect.World = Matrix.Identity;
            quadEffect.View = view;
            quadEffect.Projection = projection;
            quadEffect.TextureEnabled = true;
            quadEffect.Texture = grass;

            foreach (EffectPass pass in quadEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (TerrainTile t in _terrainTiles)
                {
                    GraphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        t.Vertices, 0, 4,
                        TerrainTile.Indexes, 0, 2);
                }
            }
            // /

            base.Draw(gameTime);
        }
    }
}