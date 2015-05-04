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
using AMXWrapper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Services
{
    public class GameWorldService : DrawableGameComponent, IGameWorldService
    {
        private readonly BasicEffect _basicEffect;
        private readonly ICameraService _cameraService;
        private readonly BoundlessQuadTree _entities = new BoundlessQuadTree();
        private readonly Dictionary<string, Graph> _graphsByName = new Dictionary<string, Graph>();
        private int _agentId;

        public GameWorldService(Game game, ICameraService cameraService) : base(game)
        {
            if (cameraService == null) throw new ArgumentNullException("cameraService");

            _cameraService = cameraService;
            _basicEffect = new BasicEffect(GraphicsDevice);
        }

        [ScriptingFunction]
        public bool DrawGraphs { get; set; }

        public QuadTree Entities
        {
            get { return _entities; }
        }

        public Graph this[string key]
        {
            get { return _graphsByName.ContainsKey(key) ? _graphsByName[key] : null; }
        }

        public void Add(IEntity entity)
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

        [ScriptingFunction]
        public bool CreateGraph(string name)
        {
            if (name == null || _graphsByName.ContainsKey(name)) return false;

            _graphsByName[name] = new Graph();
            return true;
        }

        [ScriptingFunction]
        public void SetTarget(float x, float y)
        {
            _cameraService.SetTarget(new Vector3(x, 0, y));
        }

        [ScriptingFunction]
        public void SetTargetEntity(int id)
        {
            var entity = _entities.FirstOrDefault(a => a.Id == id);
            _cameraService.SetTarget(entity);
        }

        [ScriptingFunction]
        public bool SendMessage(int id, int message, int contents)
        {
            var entity = _entities.FirstOrDefault(a => a.Id == id);
            var messageHandler = entity as IMessageHandler;

            if (messageHandler == null) return false;

            messageHandler.HandleMessage(message, contents);
            return true;
        }

        [ScriptingFunction]
        public int SendMessageToAll(int message, int contents)
        {
            var messageHandlers = _entities.OfType<IMessageHandler>();

            var enumerable = messageHandlers as IMessageHandler[] ?? messageHandlers.ToArray();

            foreach (var messageHandler in enumerable)
                messageHandler.HandleMessage(message, contents);

            return enumerable.Count();
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
        public int GetGraphCount()
        {
            return _graphsByName.Count;
        }

        [ScriptingFunction]
        public bool GetGraphName(int index, CellPtr namePtr, int nameLength)
        {
            if (index < 0 || index >= _graphsByName.Count)
            {
                return false;
            }
            var name = _graphsByName.Keys.ElementAt(index);

            if (name.Length >= nameLength)
                name = name.Substring(0, nameLength - 1);

            AMX.SetString(namePtr, name, false);
            return true;
        }

        [ScriptingFunction]
        public int FillGraph(string name, float minX, float minY, float maxX, float maxY, float offset)
        {
            if (name == null || !_graphsByName.ContainsKey(name)) return 0;

            var graph = _graphsByName[name];

            var init = graph.Keys.Count;
            var offsets = new[]
            {
                new Vector3(offset, 0, offset),
                new Vector3(-offset, 0, -offset),
                new Vector3(offset, 0, -offset),
                new Vector3(-offset, 0, offset),
                new Vector3(-offset, 0, 0),
                new Vector3(offset, 0, 0),
                new Vector3(0, 0, -offset),
                new Vector3(0, 0, offset)
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

            return graph.Keys.Count - init;
        }

        private void Line(Vector3 a, Vector3 b, Color c, Color d)
        {
            var vertices = new[] {new VertexPositionColor(a, c), new VertexPositionColor(b, d)};
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        #region Overrides of DrawableGameComponent

        public override void Draw(GameTime gameTime)
        {
            if (DrawGraphs)
            {
                _basicEffect.VertexColorEnabled = true;
                _basicEffect.World = Matrix.Identity;
                _basicEffect.View = _cameraService.View;
                _basicEffect.Projection = _cameraService.Projection;

                _basicEffect.CurrentTechnique.Passes[0].Apply();

                var height = new Vector3(0, 0.1f, 0);
                foreach (var graph in _graphsByName.Values)
                {
                    foreach (var node in graph.Values)
                    {
                        foreach (var n in node)
                            Line(node.Position + height, n.Target.Position + height, Color.Red, Color.GreenYellow);
                    }

                    height.Y += 0.2f;
                }
            }
        }

        #endregion
    }
}