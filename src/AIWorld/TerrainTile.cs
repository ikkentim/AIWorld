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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld
{
    internal struct TerrainTile
    {
        public const float Size = 4.0f;

        static TerrainTile()
        {
            Indexes = new[]
            {
                (short) 0,
                (short) 1,
                (short) 2,
                (short) 2,
                (short) 1,
                (short) 3
            };
        }

        public TerrainTile(Vector3 position) : this()
        {
            Position = position;
            // Fill in texture coordinates to display full texture
            // on quad
            var textureUpperLeft = new Vector2(0.0f, 0.0f);
            var textureUpperRight = new Vector2(1.0f, 0.0f);
            var textureLowerLeft = new Vector2(0.0f, 1.0f);
            var textureLowerRight = new Vector2(1.0f, 1.0f);

            Vertices = new VertexPositionNormalTexture[4];

            for (int i = 0; i < 4; i++)
            {
                Vertices[i].Normal = Vector3.Up;
            }

            Vector3 quarter = new Vector3(Size, 0, Size)/2;
            Vector3 nquarter = new Vector3(Size, 0, -Size)/2;

            Vector3 upperLeft = position + nquarter;
            Vector3 upperRight = position + quarter;
            Vector3 lowerLeft = position - quarter;
            Vector3 lowerRight = position - nquarter;

            Vertices[0].Position = lowerLeft;
            Vertices[0].TextureCoordinate = textureLowerLeft;
            Vertices[1].Position = upperLeft;
            Vertices[1].TextureCoordinate = textureUpperLeft;
            Vertices[2].Position = lowerRight;
            Vertices[2].TextureCoordinate = textureLowerRight;
            Vertices[3].Position = upperRight;
            Vertices[3].TextureCoordinate = textureUpperRight;
        }

        public Vector3 Position { get; set; }

        public static short[] Indexes { get; private set; }

        public VertexPositionNormalTexture[] Vertices { get; private set; }
    }
}