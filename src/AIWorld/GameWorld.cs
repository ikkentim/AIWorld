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

using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld
{
    internal class GameWorld
    {
        private readonly QuadTree _entities = new BoundlessQuadTree();

        public QuadTree Entities
        {
            get { return _entities; }
        }

        public void Update(Matrix view, Matrix projection, GameTime gameTime)
        {
            foreach (IEntity entity in _entities)
            {
                QuadTree tree = _entities.GetQuadTreeContainingEntity(entity);
                entity.Update(this, view, projection, gameTime);
                if (tree == null) continue;

                if (tree == _entities.FindQuadTreeForEntity(entity)) continue;

                Entities.Remove(entity);
                Entities.Insert(entity);
            }
        }

        public void Render(GraphicsDevice graphicsDevice, Matrix view, Matrix projection, GameTime gameTime)
        {
            foreach (IEntity entity in _entities)
            {
                entity.Render(graphicsDevice, view, projection, gameTime);
            }


//            foreach (AABB b in _entities.GetDebugBoxes())
//            {
//                VertexPositionColor[] vertices = new[]
//                {
//                    b.Center + new Vector3(b.HalfDimension.X, b.HalfDimension.Y, b.HalfDimension.Z),
//                    b.Center + new Vector3(b.HalfDimension.X, b.HalfDimension.Y, -b.HalfDimension.Z),
//                    b.Center + new Vector3(b.HalfDimension.X, -b.HalfDimension.Y, b.HalfDimension.Z),
//                    b.Center + new Vector3(b.HalfDimension.X, -b.HalfDimension.Y, -b.HalfDimension.Z),
//                    b.Center + new Vector3(-b.HalfDimension.X, b.HalfDimension.Y, b.HalfDimension.Z),
//                    b.Center + new Vector3(-b.HalfDimension.X, b.HalfDimension.Y, -b.HalfDimension.Z),
//                    b.Center + new Vector3(-b.HalfDimension.X, -b.HalfDimension.Y, b.HalfDimension.Z),
//                    b.Center + new Vector3(-b.HalfDimension.X, -b.HalfDimension.Y, -b.HalfDimension.Z)
//                }.Select(v => new VertexPositionColor(v, Color.Blue)).ToArray();
//
//                short[] indexes =
//                {
//                    0, 1,
//                    2, 3,
//                    4, 5,
//                    6, 7,
//                    7, 3,
//                    6, 2,
//                    4, 0,
//                    5, 1,
//                    6, 4,
//                    2, 0,
//                    3, 1,
//                    7, 5
//                };
//                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, vertices, 0, vertices.Length, indexes,
//                    0, 12);
//            }
        }
    }
}