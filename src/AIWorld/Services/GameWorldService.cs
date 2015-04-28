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
using Microsoft.Xna.Framework;

namespace AIWorld.Services
{
    public class GameWorldService : GameComponent, IGameWorldService
    {
        private readonly BoundlessQuadTree _entities;
        private readonly List<Road> _roads;

        public GameWorldService(Game game) : base(game)
        {
            _entities = new BoundlessQuadTree();
            _roads = new List<Road>();
        }

        public QuadTree Entities
        {
            get { return _entities; }
        }

        public IEnumerable<Road> Roads
        {
            get { return _roads; }
        }

        public void Add(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            Game.Components.Add(entity);
            Entities.Add(entity);
        }

        public void Add(Road road)
        {
            if (road == null) throw new ArgumentNullException("road");
            Game.Components.Add(road);
            _roads.Add(road);
        }

        #region Overrides of GameComponent

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _entities.FixPositions();
        }

        #endregion
    }
}