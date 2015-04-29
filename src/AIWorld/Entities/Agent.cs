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
using AMXWrapper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Entities
{
    public class Agent : Entity, IMovingEntity, IScripted
    {
        private const float MinimumDetectionBoxLength = 0.75f;
        private const float ArriveDecelerationTweaker = 1.3f;
        private const float AproxMaxObjectSize = 1.0f;
        private const float BreakingWeight = 0.005f;
        private readonly ICameraService _cameraService;
        private readonly IGameWorldService _gameWorldService;
        private readonly Stack<Vector3> _path = new Stack<Vector3>();
        private Model _model;
        private Matrix[] _transforms;

        public Agent(Game game, string scriptname, Vector3 position)
            : base(game)
        {
            if (scriptname == null) throw new ArgumentNullException("scriptname");
            Position = position;

            _cameraService = game.Services.GetService<ICameraService>();
            _gameWorldService = game.Services.GetService<IGameWorldService>();

            Script = new ScriptBox("agent", scriptname);
            Script.Register(this);
            Script.Register(_gameWorldService);

            Script.ExecuteMain();

            // simple default route for testing
//            var target = new Vector3(10, 0, 10);
//            var a = _gameWorldService.Graph.NearestNode(Vector3.Zero);
//            var b = _gameWorldService.Graph.NearestNode(target);
//            _path = new Stack<Vector3>(new[] {target}.Concat(_gameWorldService.Graph.ShortestPath(a, b)));
        }

        public ScriptBox Script { get; private set; }

        [ScriptingFunction]
        public void GetPosition(out float x, out float y)
        {
            x = Position.X;
            y = Position.Z;
        }

        [ScriptingFunction]
        public void ClearPathStack()
        {
            _path.Clear();
        }

        [ScriptingFunction]
        public void PopPathNode(out float x, out float y)
        {
            var node = _path.Pop();
            x = node.X;
            y = node.Z;
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

        #region Steering behaviour

        private float DetectionBoxLength
        {
            get { return MinimumDetectionBoxLength + (Velocity.Length()/MaxSpeed)*MinimumDetectionBoxLength; }
        }

        private Vector3 Seek(Vector3 target)
        {
            return (target - Position).Truncate(MaxSpeed) - Velocity;
        }

        private Vector3 Arrive(Vector3 target)
        {
            var toTarget = target - Position;
            var distance = toTarget.Length();

            if (distance > 0.00001)
            {
                var speed = distance/(ArriveDecelerationTweaker);
                speed = Math.Min(speed, MaxSpeed);
                var desiredVelocity = toTarget*speed/distance;

                return desiredVelocity - Velocity;
            }

            return Vector3.Zero;
        }

        private Vector3 AvoidObstacles()
        {
            var bLength = DetectionBoxLength;

            var entities =
                _gameWorldService.Entities.Query(new AABB(Position, new Vector3(bLength + AproxMaxObjectSize)))
                    .Where(e => e != this && (e.Position - Position).Length() < e.Size + bLength);

            IEntity closest = null;
            var closestDistance = float.MaxValue;
            var localPositionOfClosestPoint = Vector3.Zero;

            foreach (var e in entities)
            {
                var localPoint = Transform.ToLocalSpace(Position, Heading, Vector3.Up, Side, e.Position);
                if (localPoint.X > 0)
                {
                    var combinedSize = e.Size + Size;

                    if (Math.Abs(localPoint.Z) < combinedSize)
                    {
                        var sqrtpart = (float) Math.Sqrt(combinedSize*combinedSize - localPoint.Z*localPoint.Z);

                        var ip = sqrtpart <= localPoint.X ? localPoint.X - sqrtpart : localPoint.X + sqrtpart;

                        if (ip < closestDistance)
                        {
                            closestDistance = ip;
                            closest = e;
                            localPositionOfClosestPoint = localPoint;
                        }
                    }
                }
            }

            if (closest == null)
                return Vector3.Zero;

            var multiplier = 1 + (bLength - localPositionOfClosestPoint.X)/bLength;

            return
                Transform.VectorToWorldSpace(Heading, Vector3.Up, Side,
                    new Vector3((closest.Size - localPositionOfClosestPoint.X)*BreakingWeight, 0,
                        closest.Size - localPositionOfClosestPoint.Z*multiplier));
        }

        private Vector3 CalculateSteeringForce()
        {
            if (_path == null || !_path.Any()) return Vector3.Zero;

            var target = _path.Peek();
            var force = Vector3.Zero;

            if (_path.Count > 1)
                force += Seek(target)*0.9f;
            else
                force += Arrive(target)*0.9f;

            force += AvoidObstacles()*1.6f;

            return force.Truncate(MaxForce);
        }

        #endregion

        #region Implementation of IMovingEntity

        public Vector3 Velocity { get; private set; }

        [ScriptingFunction]
        public float Mass { get; set; }

        public Vector3 Heading { get; private set; }

        public Vector3 Side { get; private set; }

        [ScriptingFunction]
        public float MaxSpeed { get; set; }

        [ScriptingFunction]
        public float MaxForce { get; set; }

        //[ScriptingFunction]
        //public float MaxTurnRate { get; private set; }

        #endregion

        #region Overrides of Entity

        public override Vector3 Position { get; set; }

        [ScriptingFunction]
        public override float Size { get; set; }

        #endregion
    }
}