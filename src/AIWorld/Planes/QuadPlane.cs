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

using AIWorld.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Planes
{
    public class QuadPlane : DrawableGameComponent
    {
        private static readonly short[] Indexes = {0, 1, 2, 2, 1, 3};
        private readonly ICameraService _cameraService;
        private readonly BasicEffect _effect;
        private readonly VertexPositionNormalTexture[] _vertices;

        public QuadPlane(Game game, Vector3 position, float size, PlaneRotation textureRotation,
            Texture2D texture) : base(game)
        {
            _cameraService = game.Services.GetService<ICameraService>();
            _effect = new BasicEffect(game.GraphicsDevice) {TextureEnabled = true, Texture = texture};
//            _effect.EnableDefaultLighting();

            var corners = new[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f)
            };

            _vertices = new VertexPositionNormalTexture[4];

            for (var i = 0; i < 4; i++) _vertices[i].Normal = Vector3.Up;

            var quarter = new Vector3(size, 0, size)/2;
            var nquarter = new Vector3(size, 0, -size)/2;

            var offset = (int) textureRotation;
            _vertices[0].Position = position - quarter;
            _vertices[0].TextureCoordinate = corners[(3 + offset)%4];
            _vertices[1].Position = position + nquarter;
            _vertices[1].TextureCoordinate = corners[(0 + offset)%4];
            _vertices[2].Position = position - nquarter;
            _vertices[2].TextureCoordinate = corners[(2 + offset)%4];
            _vertices[3].Position = position + quarter;
            _vertices[3].TextureCoordinate = corners[(1 + offset)%4];
        }

        public QuadPlane(Game game, Vector3 position, float width, float height, PlaneRotation textureRotation,
            Texture2D texture) : base(game)
        {
            _cameraService = game.Services.GetService<ICameraService>();
            _effect = new BasicEffect(game.GraphicsDevice) {TextureEnabled = true, Texture = texture};
//            _effect.EnableDefaultLighting();

            var corners = new[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f)
            };

            _vertices = new VertexPositionNormalTexture[4];

            for (var i = 0; i < 4; i++) _vertices[i].Normal = Vector3.Up;

            var quarter = new Vector3(width, 0, height)/2;
            var nquarter = new Vector3(width, 0, -height)/2;

            var offset = (int) textureRotation;
            _vertices[0].Position = position - quarter;
            _vertices[0].TextureCoordinate = corners[(3 + offset)%4];
            _vertices[1].Position = position + nquarter;
            _vertices[1].TextureCoordinate = corners[(0 + offset)%4];
            _vertices[2].Position = position - nquarter;
            _vertices[2].TextureCoordinate = corners[(2 + offset)%4];
            _vertices[3].Position = position + quarter;
            _vertices[3].TextureCoordinate = corners[(1 + offset)%4];
        }

        public QuadPlane(Game game, Vector3 upperLeft, Vector3 upperRight, Vector3 lowerLeft,
            Vector3 lowerRight, PlaneRotation textureRotation,
            Texture2D texture) : base(game)
        {
            _cameraService = game.Services.GetService<ICameraService>();
            _effect = new BasicEffect(game.GraphicsDevice) {TextureEnabled = true, Texture = texture};
//            _effect.EnableDefaultLighting();

            var corners = new[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f)
            };

            _vertices = new VertexPositionNormalTexture[4];

            for (var i = 0; i < 4; i++) _vertices[i].Normal = Vector3.Up;

            var offset = (int) textureRotation;
            _vertices[0].Position = lowerLeft;
            _vertices[0].TextureCoordinate = corners[(3 + offset)%4];
            _vertices[1].Position = upperLeft;
            _vertices[1].TextureCoordinate = corners[(0 + offset)%4];
            _vertices[2].Position = lowerRight;
            _vertices[2].TextureCoordinate = corners[(2 + offset)%4];
            _vertices[3].Position = upperRight;
            _vertices[3].TextureCoordinate = corners[(1 + offset)%4];
        }

        public override void Draw(GameTime gameTime)
        {
            _effect.World = Matrix.Identity;
            _effect.View = _cameraService.View;
            _effect.Projection = _cameraService.Projection;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Game.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertices, 0, 4,
                    Indexes, 0, 2);
            }
            base.Draw(gameTime);
        }
    }
}