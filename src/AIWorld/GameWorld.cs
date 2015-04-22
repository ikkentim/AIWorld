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
    interface IGameWorld
    {
        QuadTree Entities { get; }

    }

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
                var position = entity.Position;
                QuadTree tree = _entities.GetQuadTreeContainingEntity(entity);
                entity.Update(this, view, projection, gameTime);
                if (tree == null || position == entity.Position) continue;

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

        }
    }
}