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
using AIWorld.Helpers;
using AIWorld.Scripting;
using AIWorld.Services;
using AIWorld.Steering;
using AMXWrapper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AIWorld.Entities
{
    public class Agent : Entity, IMovingEntity, IScripted
    {
        private readonly BasicEffect _basicEffect;
        private readonly ICameraService _cameraService;
        private readonly IGameWorldService _gameWorldService;
        private readonly AMXPublic _onUpdate;
        private readonly AMXPublic _onMouseClick;
        private readonly AMXPublic _onClicked;
        private readonly AMXPublic _onKeyStateChanged;
        private readonly Stack<Node> _path = new Stack<Node>();

        private readonly Dictionary<string, WeightedSteeringBehavior> _steeringBehaviors =
            new Dictionary<string, WeightedSteeringBehavior>();

        private Model _model;
        private float _targetRange;
        private float _targetRangeSquared;
        private Matrix[] _transforms;

        public Agent(Simulation game, string scriptname, Vector3 position)
            : base(game)
        {
            if (scriptname == null) throw new ArgumentNullException("scriptname");

            _basicEffect = new BasicEffect(GraphicsDevice);

            game.KeyStateChanged += game_KeyStateChanged;
            game.MouseClick += game_MouseClick;
            Position = position;

            _cameraService = game.Services.GetService<ICameraService>();
            _gameWorldService = game.Services.GetService<IGameWorldService>();

            Script = new ScriptBox("agent", scriptname);
            Script.Register(this, _gameWorldService, game.Services.GetService<IConsoleService>());

            _onUpdate = Script.FindPublic("OnUpdate");
            _onClicked = Script.FindPublic("OnClicked");
            _onMouseClick = Script.FindPublic("OnMouseClick");
            _onKeyStateChanged = Script.FindPublic("OnKeyStateChanged");

        }

        public void Initialize()
        {
            Script.ExecuteMain();
        }

        private void game_MouseClick(object sender, MouseClickEventArgs e)
        {
            if (_onMouseClick == null) return;

            // Push the arguments and call the callback.
            Script.Push(e.Position.Z);
            Script.Push(e.Position.X);
            Script.Push(e.Button);

            _onMouseClick.Execute();
        }

        private void game_KeyStateChanged(object sender, KeyStateEventArgs e)
        {
            if (_onKeyStateChanged == null) return;

            // Allocate an array for new keys and old keys in the abstract machine.
            var newKeys = Script.Allot(e.NewKeys.Length + 1);
            var oldKeys = Script.Allot(e.OldKeys.Length + 1);

            // Copy the keys to the machine.
            for (var i = 0; i < e.NewKeys.Length; i++)
                (newKeys + i).Set((int)e.NewKeys[i]);

            for (var i = 0; i < e.OldKeys.Length; i++)
                (oldKeys + i).Set((int)e.OldKeys[i]);

            (newKeys + e.NewKeys.Length).Set((int)Keys.None);
            (oldKeys + e.OldKeys.Length).Set((int)Keys.None);

            // Push the arguments and call the callback.
            Script.Push(oldKeys);
            Script.Push(newKeys);

            _onKeyStateChanged.Execute();

            // Release the arrays.
            Script.Release(newKeys);
            Script.Release(oldKeys);
        }

        public override bool OnClicked(MouseClickEventArgs e)
        {
            if (_onClicked == null) return false;

            // Push the arguments and call the callback.
            Script.Push(e.Position.Z);
            Script.Push(e.Position.X);
            Script.Push(e.Button);
            return _onClicked.Execute() == 1;
        }

        [ScriptingFunction]
        public override int Id { get; set; }

        [ScriptingFunction]
        public bool DrawPath { get; set; }

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

            x = node.Position.X;
            y = node.Position.Z;
            return true;
        }

        [ScriptingFunction]
        public void PushPathNode(float x, float y)
        {
            _path.Push(new Node(new Vector3(x, 0, y)));
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

            x = node.Position.X;
            y = node.Position.Z;
            return true;
        }

        [ScriptingFunction]
        public bool PushPath(string key, float startx, float starty, float endx, float endy)
        {
            var a = new Vector3(startx, 0, starty);
            var b = new Vector3(endx, 0, endy);

            var l = _path.Count;

            var graph = _gameWorldService[key];
            if (graph == null) return false;

            foreach (var n in _gameWorldService[key].ShortestPath(a, b))
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

        private void Line(Vector3 a, Vector3 b, Color c, Color d)
        {
            var vertices = new[] { new VertexPositionColor(a, c), new VertexPositionColor(b, d) };
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        #region Overrides of GameComponent

        public override void Update(GameTime gameTime)
        {
            if (_onUpdate != null)
            {
                _onUpdate.Execute();
            }

            UpdatePosition(gameTime);

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

            if (DrawPath)
            {
                _basicEffect.VertexColorEnabled = true;
                _basicEffect.World = Matrix.Identity;
                _basicEffect.View = _cameraService.View;
                _basicEffect.Projection = _cameraService.Projection;

                _basicEffect.CurrentTechnique.Passes[0].Apply();

                var height = new Vector3(0, 0.5f, 0);
                foreach (var node in _path)
                {
                    if (node.Previous == null) continue;

                    foreach (var edge in node.Where(e => e.Target.Previous == node))
                        Line(edge.Target.Position + height, node.Position + height, Color.Red, Color.Red);

                    Line(node.Previous.Position + height, node.Position + height, Color.Blue, Color.Blue);
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