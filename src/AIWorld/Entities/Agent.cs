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
using AIWorld.Helpers;
using AIWorld.Scripting;
using AIWorld.Services;
using AIWorld.Steering;
using AMXWrapper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Entities
{
    public class Agent : Entity, IMovingEntity, IScripted
    {
        private readonly ICameraService _cameraService;
        private readonly IGameWorldService _gameWorldService;
        private readonly AMXPublic _onUpdate;
        private readonly Stack<Vector3> _path = new Stack<Vector3>();

        private readonly Dictionary<string, WeightedSteeringBehavior> _steeringBehaviors =
            new Dictionary<string, WeightedSteeringBehavior>();

        private Model _model;
        private float _targetRange;
        private float _targetRangeSquared;
        private Matrix[] _transforms;

        public Agent(Game game, string scriptname, Vector3 position)
            : base(game)
        {
            if (scriptname == null) throw new ArgumentNullException("scriptname");
            Position = position;

            _cameraService = game.Services.GetService<ICameraService>();
            _gameWorldService = game.Services.GetService<IGameWorldService>();

            Script = new ScriptBox("agent", scriptname);
            Script.Register(this, _gameWorldService, game.Services.GetService<IConsoleService>());

            _onUpdate = Script.FindPublic("OnUpdate");

            Script.ExecuteMain();
        }

        [ScriptingFunction]
        public override int Id { get; set; }

        [ScriptingFunction]
        public float TargetRange
        {
            get { return _targetRange; }
            set
            {
                _targetRange = value;
                _targetRangeSquared = value*value;
            }
        }

        public ScriptBox Script { get; private set; }

        [ScriptingFunction]
        public bool ClearPathStack()
        {
            if (_path.Count == 0) return false;

            _path.Clear();
            return true;
        }

        [ScriptingFunction]
        public bool PopPathNode(out float x, out float y)
        {
            if (_path.Count == 0)
            {
                x = 0;
                y = 0;
                return false;
            }

            var node = _path.Pop();

            x = node.X;
            y = node.Z;
            return true;
        }

        [ScriptingFunction]
        public void PushPathNode(float x, float y)
        {
            _path.Push(new Vector3(x, 0, y));
        }

        [ScriptingFunction]
        public bool PeekPathNode(out float x, out float y)
        {
            if (_path.Count == 0)
            {
                x = 0;
                y = 0;
                return false;
            }

            var node = _path.Peek();

            x = node.X;
            y = node.Z;
            return true;
        }

        [ScriptingFunction]
        public bool PushPath(float startx, float starty, float endx, float endy)
        {
            var a = new Vector3(startx, 0, starty);
            var b = new Vector3(endx, 0, endy);

            var l = _path.Count;

            foreach (var n in _gameWorldService.Graph.ShortestPath(a, b))
                _path.Push(n);

            return _path.Count > l;
        }

        [ScriptingFunction]
        public void SetModel(string modelname)
        {
            _model = Game.Content.Load<Model>(modelname);

            _transforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(_transforms);
        }

        [ScriptingFunction]
        public void AddSteeringBehavior(string key, SteeringBehaviorType type, float weight, float x, float y)
        {
            if (key == null) throw new ArgumentNullException("key");

            switch (type)
            {
                case SteeringBehaviorType.Arrive:
                    _steeringBehaviors[key] =
                        new WeightedSteeringBehavior(new ArriveSteeringBehavior(this, new Vector3(x, 0, y)), weight);
                    break;
                case SteeringBehaviorType.Seek:
                    _steeringBehaviors[key] =
                        new WeightedSteeringBehavior(new SeekSteeringBehavior(this, new Vector3(x, 0, y)), weight);
                    break;
                case SteeringBehaviorType.ObstacleAvoidance:
                    _steeringBehaviors[key] = new WeightedSteeringBehavior(new AvoidObstaclesBehavior(this), weight);
                    break;
                case SteeringBehaviorType.Explore:
                    _steeringBehaviors[key] =
                        new WeightedSteeringBehavior(new ExploreSteeringBehavior(this, new Vector3(x, 0, y)), weight);
                    break;
                default:
                    throw new Exception("Invalid steering behaviour");
            }
        }

        [ScriptingFunction]
        public bool RemoveSteeringBehavior(string key)
        {
            return _steeringBehaviors.Remove(key);
        }

        public bool IsInRangeOfPoint(Vector3 point, float range)
        {
            return (point - Position).Length() < range;
        }

        public bool IsInTargetRangeOfPoint(Vector3 point)
        {
            return (point-Position).LengthSquared() < _targetRangeSquared;
        }

        [ScriptingFunction]
        public bool IsInRangeOfPoint(float x, float y, float range)
        {
            return IsInRangeOfPoint(new Vector3(x, 0, y), range);
        }

        [ScriptingFunction]
        public bool IsInTargetRangeOfPoint(float x, float y)
        {
            return IsInTargetRangeOfPoint(new Vector3(x, 0, y));
        }

        private Vector3 CalculateSteeringForce()
        {
            return
                _steeringBehaviors.Values.Aggregate(Vector3.Zero,
                    (current, behavior) => current + behavior.Behavior.Calculate()*behavior.Weight).Truncate(MaxForce);
        }

        private void UpdatePosition(GameTime gameTime)
        {
            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            var steeringForce = CalculateSteeringForce();

            var acceleration = steeringForce/Mass;
            Velocity += acceleration*deltaTime;

            Velocity = Velocity.Truncate(MaxSpeed);

            Position += Velocity*deltaTime;

            if (Velocity.LengthSquared() > 0.00001)
            {
                Heading = Vector3.Normalize(Velocity);
                Side = Heading.RotateAboutOriginY(Vector3.Zero, MathHelper.ToRadians(90));
            }
        }

        private void UpdateTarget()
        {
            if (_path == null || !_path.Any()) return;
            if (_path.Count == 1)
            {
                if (Velocity.LengthSquared() < 0.001)
                {
                    Velocity = Vector3.Zero;
                    _path.Pop();

                    try
                    {
                        Script.FindPublic("OnPathEnd").Execute();
                    }
                    catch (AMXException)
                    {
                    }
                }
            }
            else if ((Position - _path.Peek()).LengthSquared() < 0.9f) _path.Pop();
        }

        #region Overrides of GameComponent

        public override void Update(GameTime gameTime)
        {
            if (_onUpdate != null)
            {
                _onUpdate.Execute();
            }

            UpdatePosition(gameTime);
            UpdateTarget();

            base.Update(gameTime);
        }

        #endregion

        #region Overrides of DrawableGameComponent

        public override void Draw(GameTime gameTime)
        {
            if (_model != null)
            {
                foreach (var mesh in _model.Meshes)
                {
                    foreach (var effect in mesh.Effects.Cast<BasicEffect>())
                    {
                        effect.World = _transforms[mesh.ParentBone.Index]*Matrix.CreateRotationY(Heading.GetYAngle())*
                                       Matrix.CreateTranslation(Position);
                        effect.View = _cameraService.View;
                        effect.Projection = _cameraService.Projection;
                        effect.EnableDefaultLighting();
                    }
                    mesh.Draw();
                }
            }
            base.Draw(gameTime);
        }

        #endregion

        #region Implementation of IMovingEntity

        public Vector3 Velocity { get; private set; }

        [ScriptingFunction]
        public float Mass { get; set; }

        [ScriptingFunction]
        public Vector3 Heading { get; private set; }

        public Vector3 Side { get; private set; }

        [ScriptingFunction]
        public float MaxSpeed { get; set; }

        [ScriptingFunction]
        public float MaxForce { get; set; }

        #endregion

        #region Overrides of Entity

        [ScriptingFunction]
        public override Vector3 Position { get; set; }

        [ScriptingFunction]
        public override float Size { get; set; }

        #endregion
    }
}