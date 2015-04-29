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
using AIWorld.Core;
using AIWorld.Entities;
using AIWorld.Scripting;
using AMXWrapper;
using Microsoft.Xna.Framework;

namespace AIWorld.Services
{
    public class GameWorldService : GameComponent, IGameWorldService
    {
        private readonly BoundlessQuadTree _entities = new BoundlessQuadTree();
        private readonly Graph _graph = new Graph();

        public GameWorldService(Game game) : base(game)
        {
        }

        public QuadTree Entities
        {
            get { return _entities; }
        }

        public Graph Graph
        {
            get { return _graph; }
        }

        public void Add(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            Game.Components.Add(entity);
            Entities.Add(entity);
        }

        public void Add(QuadPlane plane)
        {
            if (plane == null) throw new ArgumentNullException("plane");
            Game.Components.Add(plane);
        }

        #region Overrides of GameComponent

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _entities.FixPositions();
        }

        #endregion

        public void Register(ScriptBox scriptBox)
        {
            scriptBox.Register<float, float, CellPtr, CellPtr>(GetClosestNode);
        }

        private int GetClosestNode(float x, float y, CellPtr tx, CellPtr ty)
        {
            var closest = Graph.NearestNode(new Vector3(x, 0, y));

            tx.Set(Cell.FromFloat(closest.X));
            ty.Set(Cell.FromFloat(closest.Z));
            return 1;
        }
    }
}