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
using System.Diagnostics;
using System.Linq;
using AIWorld.Core;
using AIWorld.Entities;
using AIWorld.Scripting;
using Microsoft.Xna.Framework;

namespace AIWorld.Services
{
    public class GameWorldService : GameComponent, IGameWorldService
    {
        private readonly BoundlessQuadTree _entities = new BoundlessQuadTree();
        private readonly Dictionary<string, Graph> _graphsByName = new Dictionary<string, Graph>();
        private int _agentId;

        public GameWorldService(Game game) : base(game)
        {
        }

        public QuadTree Entities
        {
            get { return _entities; }
        }

        public Graph this[string key]
        {
            get
            {
                return _graphsByName.ContainsKey(key) ? _graphsByName[key] : null;
            }
        }

        public void Add(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            entity.Id = _agentId++;
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

        [ScriptingFunction]
        public int GetClosestNode(string key, float x, float y, out float nx, out float ny)
        {
            var closest = this[key].NearestNode(new Vector3(x, 0, y));

            nx = closest.X;
            ny = closest.Z;
            return 1;
        }

        [ScriptingFunction]
        public bool AddGraphNode(string key, float x1, float y1, float x2, float y2)
        {
            var graph = this[key];
            if (graph == null) return false;

            graph.Add(new Vector3(x1, 0, y1), new Vector3(x2, 0, y2));
            return true;
        }

        [ScriptingFunction]
        public bool CreateGraph(string name)
        {
            if (name == null || _graphsByName.ContainsKey(name)) return false;

            _graphsByName[name] = new Graph();
            return true;
        }

        public bool IsPointOccupied(Vector3 point)
        {
            return
                Entities.Query(new AABB(point, new Vector3(WorldObject.MaxSize)))
                    .Any(e => (e.Position - point).Length() < e.Size);
        }

        [ScriptingFunction]
        public bool IsPointOccupied(float x, float y)
        {
            return IsPointOccupied(new Vector3(x, 0, y));
        }
        [ScriptingFunction]
        public bool FillGraph(string name, float minX, float minY, float maxX, float maxY, float offset)
        {
            if (name == null || !_graphsByName.ContainsKey(name)) return false;

            var graph = _graphsByName[name];

            var init = graph.Keys.Count;
            var offsets = new[]
            {
                new Vector3(offset, 0, offset),
                new Vector3(-offset, 0, -offset),
                new Vector3(offset, 0, -offset),
                new Vector3(-offset, 0, offset)
            };
            for (var x = minX; x <= maxX; x += offset)
                for (var y = minY; y <= maxY; y += offset)
                {
                    var point = new Vector3(x, 0, y);
                    if (IsPointOccupied(point)) continue;

                    foreach (
                        var p in
                            offsets.Select(o => o + point)
                                .Where(
                                    p => p.X >= minX && p.X <= maxX && p.Z >= minY && p.Z <= maxY && !IsPointOccupied(p))
                        )
                        graph.Add(point, p);
                }

            Debug.WriteLine("Created nodes: {0}", graph.Keys.Count-init);
            return true;
        }
    }
}