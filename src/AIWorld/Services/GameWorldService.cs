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
using AIWorld.Planes;
using AIWorld.Scripting;
using AMXWrapper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Services
{
    public class GameWorldService : DrawableGameComponent, IGameWorldService
    {
        private readonly BasicEffect _basicEffect;
        private readonly ICameraService _cameraService;
        private readonly IConsoleService _consoleService;
        private readonly BoundlessQuadTree _entities = new BoundlessQuadTree();
        private readonly Dictionary<string, Graph> _graphsByName = new Dictionary<string, Graph>();
        private readonly Dictionary<string, object> _variables = new Dictionary<string, object>(); 
        private int _entityId;

        public Simulation Simulation { get; private set; }

        #region Constructors

        public GameWorldService(Simulation simulation, ICameraService cameraService, IConsoleService consoleService) : base(simulation)
        {
            if (simulation == null) throw new ArgumentNullException("simulation");
            if (cameraService == null) throw new ArgumentNullException("cameraService");

            Simulation = simulation;
            _cameraService = cameraService;
            _consoleService = consoleService;
            _basicEffect = new BasicEffect(GraphicsDevice);
        }

        #endregion

        #region Overrides of GameComponent

        /// <summary>
        /// Shuts down the component.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _basicEffect.Dispose();

                _graphsByName.Clear();
                _entities.Clear();
            }

            base.Dispose(disposing);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _entities.FixPositions();
        }

        #endregion

        #region Implementation of IGameWorldService

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

            entity.Id = _entityId++;
            Game.Components.Add(entity);
            Entities.Add(entity);
        }

        public void Add(QuadPlane plane)
        {
            if (plane == null) throw new ArgumentNullException("plane");
            Game.Components.Add(plane);
        }

        [ScriptingFunction] // Also available to API
        public bool CreateGraph(string name)
        {
            if (name == null || _graphsByName.ContainsKey(name)) return false;

            _graphsByName[name] = new Graph();
            return true;
        }

        #endregion

        #region Methods of GameWorldService

        /// <summary>
        ///     Draws a line between the specified points in the specified color.
        /// </summary>
        /// <param name="point1">The point1.</param>
        /// <param name="point2">The point2.</param>
        /// <param name="color1">The color1.</param>
        /// <param name="color2">The color2.</param>
        private void DrawLine(Vector3 point1, Vector3 point2, Color color1, Color color2)
        {
            var vertices = new[] { new VertexPositionColor(point1, color1), new VertexPositionColor(point2, color2) };
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        #endregion

        #region API

        #region API - Debugging

        [ScriptingFunction]
        public bool DrawGraphs { get; set; }

        #endregion

        #region API - Graphs

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

        #endregion

        #region API - Camera

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

        #endregion

        #region API - Entity

        [ScriptingFunction]
        public bool GetEntityPosition(int id, out float x, out float y)
        {
            var entity = _entities.FirstOrDefault(a => a.Id == id);

            if (entity == null)
            {
                x = 0;
                y = 0;
                return false;
            }

            x = entity.Position.X;
            y = entity.Position.Z;
            return true;
        }

        [ScriptingFunction]
        public float GetEntitySize(int id)
        {
            var entity = _entities.FirstOrDefault(a => a.Id == id);

            return entity == null ? 0 : entity.Size;
        }

        #endregion

        #region API - WorldObject

        [ScriptingFunction]
        public int FindNearestWorldObject(float x, float y, string model)
        {
            var result = _entities.OfType<WorldObject>()
                .Where(w => model.Length == 0 || w.ModelName == model)
                .OrderBy(w => Vector3.DistanceSquared(w.Position, new Vector3(x, 0, y)))
                .FirstOrDefault();

            return result == null ? -1 : result.Id;
        }

        [ScriptingFunction]
        public bool SetWorldObjectScale(int id, float x, float y, float z)
        {
            var worldObject = _entities.OfType<WorldObject>().FirstOrDefault(w => w.Id == id);

            if (worldObject == null) return false;

            worldObject.Scale = new Vector3(x, y, z);
            return true;
        }
        [ScriptingFunction]
        public bool SetWorldObjectRotation(int id, float x, float y, float z)
        {
            var worldObject = _entities.OfType<WorldObject>().FirstOrDefault(w => w.Id == id);

            if (worldObject == null) return false;

            worldObject.Rotation = new Vector3(x, y, z);
            return true;
        }
        [ScriptingFunction]
        public bool SetWorldObjectTranslation(int id, float x, float y, float z)
        {
            var worldObject = _entities.OfType<WorldObject>().FirstOrDefault(w => w.Id == id);

            if (worldObject == null) return false;

            worldObject.Translation = new Vector3(x, y, z);
            return true;
        }

        [ScriptingFunction]
        public bool RemoveWorldObject(int id)
        {
            var worldObject = _entities.OfType<WorldObject>().FirstOrDefault(w => w.Id == id);

            if (worldObject == null) return false;

            _entities.Remove(worldObject);
            Game.Components.Remove(worldObject);
            return true; 
        }
        #endregion

        #region API - Agent

        public IEnumerable<Agent> GetNearestAgents(float x, float y, float range, string scriptName,
            Func<Agent, bool> predicate)
        {
            return _entities.Query(new AABB(new Vector3(x, 0, y), new Vector3(range)))
                .OfType<Agent>()
                .Where(a => string.IsNullOrEmpty(scriptName) || a.ScriptName == scriptName)
                .Where(predicate)
                .OrderBy(a => Vector3.DistanceSquared(a.Position, new Vector3(x, 0, y)));
        }

        public int FindNearestAgent(float x, float y, float range, string scriptName, Func<Agent, bool> predicate)
        {
            var result = GetNearestAgents(x, y, range, scriptName, predicate).FirstOrDefault();

            return result == null ? -1 : result.Id;
        }

        [ScriptingFunction]
        public int FindNearestAgent(float x, float y, float range, string scriptName)
        {
            return FindNearestAgent(x, y, range, scriptName, a => true);
        }

        [ScriptingFunction]
        public int FindNearestAgentByVar(float x, float y, float range, string key, int value, string scriptName)
        {
            return FindNearestAgent(x, y, range, scriptName, a => a.GetVar(key) == value);
        }

        [ScriptingFunction]
        public int FindNearestAgentByVarFloat(float x, float y, float range, string key, float value, string scriptName)
        {
            return FindNearestAgent(x, y, range, scriptName, a => a.GetVarFloat(key) == value);
        }

        [ScriptingFunction]
        public int FindNearestAgentByVarString(float x, float y, float range, string key, string value, string scriptName)
        {
            return FindNearestAgent(x, y, range, scriptName, a => a.GetVarString(key) == value);
        }

        [ScriptingFunction]
        public int GetNearestAgents(float x, float y, float range, string scriptName, CellPtr array, int length)
        {
            if (length < 0)
                return 0;

            var idx = 0;
            foreach (var agent in GetNearestAgents(x, y, range, scriptName, a => true).Take(length))
                array[idx] = agent.Id;

            return idx;
        }

        [ScriptingFunction]
        public int GetNearestAgentsByVar(float x, float y, float range, string key, int value, string scriptName, CellPtr array, int length)
        {
            if (length < 0)
                return 0;

            var idx = 0;
            foreach (var agent in GetNearestAgents(x, y, range, scriptName, a => a.GetVar(key) == value).Take(length))
                array[idx] = agent.Id;

            return idx;
        }

        [ScriptingFunction]
        public int GetNearestAgentsByVarFloat(float x, float y, float range, string key, float value, string scriptName, CellPtr array, int length)
        {
            if (length < 0)
                return 0;

            var idx = 0;
            foreach (var agent in GetNearestAgents(x, y, range, scriptName, a => a.GetVarFloat(key) == value).Take(length))
                array[idx] = agent.Id;

            return idx;
        }

        [ScriptingFunction]
        public int GetNearestAgentsByVarString(float x, float y, float range, string key, string value, string scriptName, CellPtr array, int length)
        {
            if (length < 0)
                return 0;

            var idx = 0;
            foreach (var agent in GetNearestAgents(x, y, range, scriptName, a => a.GetVarString(key) == value).Take(length))
                array[idx] = agent.Id;

            return idx;
        }

        [ScriptingFunction]
        public bool SetAgentVar(int agentid, string key, int value)
        {
            var agent = _entities.FirstOrDefault(a => a.Id == agentid) as Agent;
            if (agent == null) return false;

            agent.SetVar(key, value);
            return true;
        }

        [ScriptingFunction]
        public bool SetAgentVarFloat(int agentid, string key, float value)
        {
            var agent = _entities.FirstOrDefault(a => a.Id == agentid) as Agent;
            if (agent == null) return false;

            agent.SetVarFloat(key, value);
            return true;
        }

        [ScriptingFunction]
        public bool SetAgentVarString(int agentid, string key, string value)
        {
            var agent = _entities.FirstOrDefault(a => a.Id == agentid) as Agent;
            if (agent == null) return false;

            agent.SetVarString(key, value);
            return true;
        }

        [ScriptingFunction]
        public bool DeleteAgentVar(int agentid, string key)
        {
            var agent = _entities.FirstOrDefault(a => a.Id == agentid) as Agent;
            return agent != null && agent.DeleteVar(key);
        }

        [ScriptingFunction]
        public int GetAgentVar(int agentid, string key)
        {
            var agent = _entities.FirstOrDefault(a => a.Id == agentid) as Agent;
            return agent == null ? 0 : agent.GetVar(key);
        }

        [ScriptingFunction]
        public float GetAgentVarFloat(int agentid, string key)
        {
            var agent = _entities.FirstOrDefault(a => a.Id == agentid) as Agent;
            return agent == null ? 0 : agent.GetVarFloat(key);
        }

        [ScriptingFunction]
        public int GetAgentVarString(int agentid, string key, CellPtr retval, int length)
        {
            var agent = _entities.FirstOrDefault(a => a.Id == agentid) as Agent;
            return agent == null ? -1 : agent.GetVarString(key, retval, length);
        }

        #endregion

        #region API - GVar

        [ScriptingFunction]
        public void SetGVar(string key, int value)
        {
            _variables[key] = value;
        }

        [ScriptingFunction]
        public void SetGVarFloat(string key, float value)
        {
            _variables[key] = value;
        }

        [ScriptingFunction]
        public void SetGVarString(string key, string value)
        {
            _variables[key] = value;
        }

        [ScriptingFunction]
        public bool DeleteGVar(string key)
        {
            return _variables.Remove(key);
        }

        [ScriptingFunction]
        public int GetGVar(string key)
        {
            if (!_variables.ContainsKey(key) || !(_variables[key] is int))
                return 0;
            return (int)_variables[key];
        }

        [ScriptingFunction]
        public float GetGVarFloat(string key)
        {
            if (!_variables.ContainsKey(key) || !(_variables[key] is float))
                return 0;

            return (float)_variables[key];
        }

        [ScriptingFunction]
        public int GetGVarString(string key, CellPtr retval, int length)
        {
            if (!_variables.ContainsKey(key) || !(_variables[key] is string))
                return -1;

            if (--length <= 0) return -1;

            var value = (string)_variables[key];
            AMX.SetString(retval, value.Length > length ? value.Substring(0, length) : value, false);

            return value.Length;
        }

        #endregion

        #region API - IMessageHandler

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

        #endregion

        #region API - IScripted

        [ScriptingFunction]
        public int CallPublicFunction(AMXArgumentList arguments)
        {
            var result = 0;
            foreach (IEntity entity in _entities)
            {
                Agent s = entity as Agent;
                if (s != null)
                {
                    var retval = s.CallLocalFunction(arguments);
                    if (retval != 0) result = retval;
                }
            }

            var mainRetval = DefaultFunctions.CallFunctionOnScript(Simulation.Script, _consoleService, arguments);
            if (mainRetval != null)
                result = mainRetval.Value;

            return result;
        }

        [ScriptingFunction]
        public int CallRemoteFunction(AMXArgumentList arguments)
        {
            var retval = DefaultFunctions.CallFunctionOnScript(Simulation.Script, _consoleService, arguments);
            if (retval != null)
                return retval.Value;

            return 0;
        }

        #endregion

        #endregion

        #region Overrides of DrawableGameComponent

        public override void Draw(GameTime gameTime)
        {
            if (!DrawGraphs) return;

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
                        DrawLine(node.Position + height, n.Target.Position + height, Color.Red, Color.GreenYellow);
                }

                height.Y += 0.2f;
            }
        }

        #endregion
    }
}