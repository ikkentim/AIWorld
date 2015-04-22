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
using System.Linq;
using AIWorld.Helpers;
using AIWorld.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Entities
{
    public class Vehicle : Entity, IMovingEntity
    {
        private const float MinimumDetectionBoxLength = 0.75f;
        private const float ArriveDecelerationTweaker = 0.5f;
        private const float AproxMaxObjectSize = 1.0f;
        private const float BreakingWeight = 0.005f;
        private readonly AudioEmitter _audioEmitter;
        private readonly AudioListener _audioListener;
        private readonly BasicEffect _basicEffect;
        private readonly ICameraService _cameraService;
        private readonly IGameWorldService _gameWorldService;
        private readonly Model _model;
        private readonly Road _road;
        private readonly SoundEffectInstance _soundEffectInstance;
        private bool _istouch;
        private int _targetnode;
        private bool _targetnodeisright = true;
        private Vector3 _touch;

        public Vehicle(Vector3 position, Game game, Road road) : base(game)
        {
            _road = road;
            Position = position;

            MaxTurnRate = 2;
            MaxForce = 15.0f;
            MaxSpeed = 1.2f;
            Mass = 0.35f;
            Size = 0.2f;

            // Store parameters
            _model = game.Content.Load<Model>("models/car");
            _basicEffect = new BasicEffect(game.GraphicsDevice);
            _cameraService = game.Services.GetService<ICameraService>();
            _gameWorldService = game.Services.GetService<IGameWorldService>();
            // Setup engine sound
            _audioListener = new AudioListener
            {
                Position = Vector3.Zero,
                Up = Vector3.Up,
                Velocity = Vector3.Zero,
                Forward = Vector3.Forward
            };

            _audioEmitter = new AudioEmitter
            {
                DopplerScale = 1,
                Forward = Heading,
                Position = Position,
                Up = Vector3.Up,
                Velocity = Velocity
            };

            _soundEffectInstance = game.Content.Load<SoundEffect>(@"sounds/engine").CreateInstance();
            _soundEffectInstance.IsLooped = true;
            _soundEffectInstance.Volume = 0.2f;
            _soundEffectInstance.Apply3D(_audioListener, _audioEmitter);
            _soundEffectInstance.Play();
        }

        private float DetectionBoxLength
        {
            get { return MinimumDetectionBoxLength + (Velocity.Length()/MaxSpeed)*MinimumDetectionBoxLength; }
        }

        private void UpdateAudioPosition()
        {
            Vector3 cpos = Matrix.Invert(_cameraService.View).Translation;
            _audioListener.Position = cpos;
            _audioListener.Forward = new Vector3(_cameraService.View.M31, _cameraService.View.M32,
                _cameraService.View.M33); // Think this is the right col?
            _audioEmitter.Forward = Heading;
            _audioEmitter.Position = Position;
            _audioEmitter.Velocity = Velocity;
            _audioEmitter.DopplerScale = Math.Max(1.0f, ((Vector3.Normalize(Position - cpos) - Velocity).Length()));
            // Just testing it out

            _soundEffectInstance.Apply3D(_audioListener, _audioEmitter);
        }

        private void UpdatePosition(GameTime gameTime)
        {
            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 steeringForce = CalculateSteeringForce();

            Vector3 acceleration = steeringForce/Mass;
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
            if (
                (Position - (_targetnodeisright ? _road.RightNodes[_targetnode] : _road.LeftNodes[_targetnode])).Length() <
                0.8f)
            {
                _targetnode++;
                if (_targetnode >= _road.Nodes.Length)
                {
                    _targetnodeisright = !_targetnodeisright;
                    _targetnode = 0;
                }
            }
        }

        private Vector3 Seek(Vector3 target)
        {
            return (target - Position).Truncate(MaxSpeed) - Velocity;
        }

        private Vector3 Arrive(Vector3 target, int decel)
        {
            Vector3 toTarget = target - Position;
            float distance = toTarget.Length();

            if (distance > 0.00001)
            {
                float speed = distance/(decel*ArriveDecelerationTweaker);
                speed = Math.Min(speed, MaxSpeed);
                Vector3 desiredVelocity = toTarget*speed/distance;

                return desiredVelocity - Velocity;
            }

            return Vector3.Zero;
        }

        private Vector3 ToLocalSpace(Vector3 point)
        {
            float tx = -Vector3.Dot(Position, Heading);
            float ty = -Vector3.Dot(Position, Vector3.Up);
            float tz = -Vector3.Dot(Position, Side);

            return Vector3.Transform(point,
                new Matrix(Heading.X, 0, Side.X, 0, Heading.Y, 1, Side.Y, 0, Heading.Z, 0, Side.Z, 0, tx, ty, tz, 0));
        }

        private Vector3 VectorToWorldSpace(Vector3 vec)
        {
            return Vector3.Transform(vec,
                Matrix.Identity*
                new Matrix(Heading.X, 0, Side.X, 0, Heading.Y, 1, Side.Y, 0, Heading.Z, 0, Side.Z, 0, 0, 0, 0, 0));
        }

        private Vector3 AvoidObstacles()
        {
            float bLength = DetectionBoxLength;

            IEnumerable<IEntity> entities =
                _gameWorldService.Entities.Query(new AABB(Position, new Vector3(bLength + AproxMaxObjectSize)))
                    .Where(e => e != this && (e.Position - Position).Length() < e.Size + bLength);

            IEntity closest = null;
            float closestDistance = float.MaxValue;
            Vector3 localPositionOfClosestPoint = Vector3.Zero;

            foreach (IEntity e in entities)
            {
                Vector3 localPoint = ToLocalSpace(e.Position);
                if (localPoint.X > 0)
                {
                    float combinedSize = e.Size + Size;

                    if (Math.Abs(localPoint.Z) < combinedSize)
                    {
                        var sqrtpart = (float) Math.Sqrt(combinedSize*combinedSize - localPoint.Z*localPoint.Z);

                        float ip = sqrtpart <= localPoint.X ? localPoint.X - sqrtpart : localPoint.X + sqrtpart;

                        if (ip < closestDistance)
                        {
                            closestDistance = ip;
                            closest = e;
                            localPositionOfClosestPoint = localPoint;
                        }
                    }
                }
            }

            // debug value
            _istouch = closest != null;
            _touch = VectorToWorldSpace(localPositionOfClosestPoint);

            if (closest == null)
                return Vector3.Zero;

            float multiplier = 1 + (bLength - localPositionOfClosestPoint.X)/bLength;


            return
                VectorToWorldSpace(new Vector3((closest.Size - localPositionOfClosestPoint.X)*BreakingWeight, 0,
                    closest.Size - localPositionOfClosestPoint.Z*multiplier));
        }

        private Vector3 CalculateSteeringForce()
        {
            Vector3 target = _targetnodeisright ? _road.RightNodes[_targetnode] : _road.LeftNodes[_targetnode];

            Vector3 force = Seek(target)*1.0f;
            force += AvoidObstacles()*0.75f;

            return force.Truncate(MaxForce);
        }

        private void Line(Vector3 a, Vector3 b, Color c)
        {
            var vertices = new[] {new VertexPositionColor(a, c), new VertexPositionColor(b, c)};
            Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        #region Overrides of GameComponent

        public override void Update(GameTime gameTime)
        {
            UpdatePosition(gameTime);
            UpdateAudioPosition();
            UpdateTarget();
        }

        #endregion

        #region Overrides of DrawableGameComponent

        public override void Draw(GameTime gameTime)
        {
            var transforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index]*Matrix.CreateRotationY(Heading.GetYAngle())*
                                   Matrix.CreateTranslation(Position);
                    effect.View = _cameraService.View;
                    effect.Projection = _cameraService.Projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }

            _basicEffect.VertexColorEnabled = true;
            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = _cameraService.View;
            _basicEffect.Projection = _cameraService.Projection;

            _basicEffect.CurrentTechnique.Passes[0].Apply();

            Line(Heading*DetectionBoxLength + Position,
                Heading*DetectionBoxLength + Position + Vector3.Up, Color.Blue);

            if (_istouch)
                Line(Position + _touch, Position + _touch + Vector3.Up*3, Color.Green);

            base.Draw(gameTime);
        }

        #endregion

        #region Implementation of IEntity

        public override Vector3 Position { get; protected set; }

        public override float Size { get; protected set; }

        #endregion

        #region Implementation of IMovingEntity

        public Vector3 Velocity { get; private set; }
        public float Mass { get; private set; }
        public Vector3 Heading { get; private set; }
        public Vector3 Side { get; private set; }
        public float MaxSpeed { get; private set; }
        public float MaxForce { get; private set; }
        public float MaxTurnRate { get; private set; }

        #endregion
    }
}