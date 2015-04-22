﻿// AIWorld
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
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld
{
    internal class Vehicle : IMovingEntity
    {
        private readonly AudioEmitter _audioEmitter;
        private readonly AudioListener _audioListener;
        private readonly Model _model;
        private readonly Road _road;
        private readonly SoundEffectInstance _soundEffectInstance;
        private int _targetnode;
        private BasicEffect _basicEffect;

        public Vehicle(Vector3 position, GraphicsDevice graphicsDevice, SoundEffect engine, ContentManager content,
            Road road)
        {
            _road = road;
            _model = content.Load<Model>("models/car");
            _basicEffect = new BasicEffect(graphicsDevice);
            Position = position;
            MaxTurnRate = 2;
            MaxForce = 15.0f;
            MaxSpeed = 1.2f;
            Mass = 0.35f;
            Size = 0.2f;
            _audioListener = new AudioListener
            {
                Position = Vector3.Zero,
                Up = Vector3.Up,
                Velocity = Vector3.Zero,
                Forward = Vector3.Forward,
            };

            _audioEmitter = new AudioEmitter
            {
                DopplerScale = 1,
                Forward = Heading,
                Position = Position,
                Up = Vector3.Up,
                Velocity = Velocity,
            };

            _soundEffectInstance = engine.CreateInstance();
            _soundEffectInstance.IsLooped = true;
            _soundEffectInstance.Volume = 0.2f;
            _soundEffectInstance.Apply3D(_audioListener, _audioEmitter);
            _soundEffectInstance.Play();
        }

        private void UpdateAudioPosition(Matrix view)
        {
            Vector3 cpos = Matrix.Invert(view).Translation;
            _audioListener.Position = cpos;
            _audioListener.Forward = new Vector3(view.M31, view.M32, view.M33); // Think this is the right col?
            _audioEmitter.Forward = Heading;
            _audioEmitter.Position = Position;
            _audioEmitter.Velocity = Velocity;
            _audioEmitter.DopplerScale = Math.Max(1.0f, ((Vector3.Normalize(Position - cpos) - Velocity).Length()));
            // Just testing it out

            _soundEffectInstance.Apply3D(_audioListener, _audioEmitter);
        }

        private void UpdatePosition(GameWorld world, GameTime gameTime)
        {
            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 steeringForce = CalculateSteeringForce(world);

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
            if ((Position - _road.RightNodes[_targetnode]).Length() < 0.6f)
            {
                _targetnode++;
                _targetnode %= _road.Nodes.Length;
            }
        }

        private Vector3 Seek(Vector3 target)
        {
            Vector3 desiredVelocity = Vector3.Normalize(target - Position)*MaxSpeed;
            return desiredVelocity - Velocity;
        }

        private Vector3 Arrive(Vector3 target, int decel)
        {
            Vector3 toTarget = target - Position;
            float distance = toTarget.Length();

            if (distance > 0.00001)
            {
                const float decelerationTweaker = 0.5f;

                float speed = distance/(decel*decelerationTweaker);
                speed = Math.Min(speed, MaxSpeed);
                Vector3 desiredVelocity = toTarget*speed/distance;

                return desiredVelocity - Velocity;
            }

            return Vector3.Zero;
        }

        private Vector3 ToLocalSpace(Vector3 point)
        {
            var tx = -Vector2.Dot(new Vector2(Position.X, Position.Z), new Vector2(Heading.X, Heading.Z));
            var ty = -Vector2.Dot(new Vector2(Position.X, Position.Z), new Vector2(Side.X, Side.Z));

            Matrix transformation = new Matrix(Heading.X, Side.X, 0, 0, Heading.Z, Side.Z, 0, 0, 0, 0, 0, 0, tx, ty, 0,
                0);

            var tmp = Vector2.Transform(new Vector2(point.X, point.Z), transformation);
            return new Vector3(tmp.X, 0, tmp.Y);
        }

        private Vector3 VectorToWorldSpace(Vector3 vec)
        {
            Matrix transformation = Matrix.Identity;

            Matrix mat = Matrix.Identity;
            mat.M11 = Heading.X;
            mat.M12 = Side.X;
            mat.M21 = Heading.Z;
            mat.M22 = Side.Z;

            transformation *= mat;

            var tmp = Vector2.Transform(new Vector2(vec.X, vec.Z), transformation);
            return new Vector3(tmp.X, 0, tmp.Y);
        }

        private Vector3 AvoidObstacles(GameWorld world)
        {
            var bLength = DetectionBoxLength;
            const float aproxMaxObjectSize = 1.0f;

            var entities =
                world.Entities.Query(new AABB(Position, new Vector3(bLength + aproxMaxObjectSize)))
                    .Where(e => e != this &&  (e.Position - Position).Length() < e.Size + bLength);

            IEntity closest = null;
            float closestDistance = float.MaxValue;
            Vector3 localPositionOfClosestPoint = Vector3.Zero;

            foreach (var e in entities)
            {
                //Console.WriteLine("Hit found");
                var localPoint = ToLocalSpace(e.Position);
                //Console.WriteLine("Local point: {0}", localPoint);
                if (localPoint.X > 0)
                {
                    float expRadius = e.Size + Size;

                    if (Math.Abs(localPoint.Y) < expRadius)
                    {
                        //Console.WriteLine("is within range of path");
                        float cx = localPoint.X;
                        float cy = localPoint.Y;

                        var sqrtpart = (float) Math.Sqrt(expRadius*expRadius - cy*cy);

                        var ip = cx - sqrtpart;

                        if (ip <= 0.0)
                        {
                            ip = cx + sqrtpart;
                        }

                        if (ip < closestDistance)
                        {
                            //Console.WriteLine("is closer!!!");
                            closestDistance = ip;
                            closest = e;
                            localPositionOfClosestPoint = localPoint;
                        }
                    } 
                }
            }

            if (closest == null)
                return Vector3.Zero;

            var force = Vector3.Zero;

            var mp = 1 + (bLength - localPositionOfClosestPoint.X)/bLength;

            force.Z = closest.Size - localPositionOfClosestPoint.Z*mp;

            const float brakingWeight = 0.02f;

            force.X = (closest.Size - localPositionOfClosestPoint.X)*brakingWeight;

            //Debug.WriteLine("from {0}", force);
            var wf = VectorToWorldSpace(force);

            //Debug.WriteLine(wf);
            return wf;
        }

        private Vector3 CalculateSteeringForce(GameWorld world)
        {
            Vector3 target = _road.RightNodes[_targetnode];

            Vector3 force = Seek(target)*0.3f;
            force += AvoidObstacles(world)*0.3f;

            return force.Truncate(MaxForce);
        }

        private float DetectionBoxLength
        {
            get
            {
                const float minimumLength = 0.75f;
                return minimumLength + (Velocity.Length()/MaxSpeed)*minimumLength;
            }
        }
        private void Line(GraphicsDevice graphicsDevice, Vector3 a, Vector3 b, Color c)
        {
            var vertices = new[] { new VertexPositionColor(a, c), new VertexPositionColor(b, c) };
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }
        #region Implementation of IEntity

        public Vector3 Position { get; private set; }

        public float Size { get; set; }

        public void Update(GameWorld world, Matrix view, Matrix projection, GameTime gameTime)
        {
            UpdatePosition(world, gameTime);
            UpdateAudioPosition(view);
            UpdateTarget();
        }


        public void Render(GraphicsDevice graphicsDevice, Matrix view, Matrix projection, GameTime gameTime)
        {
            var transforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index]*Matrix.CreateRotationY(Heading.GetYAngle())*
                                   Matrix.CreateTranslation(Position);
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }

            _basicEffect.VertexColorEnabled = true;
            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = view;
            _basicEffect.Projection = projection;

            _basicEffect.CurrentTechnique.Passes[0].Apply();

            Line(graphicsDevice, Heading*DetectionBoxLength + Position,
                Heading*DetectionBoxLength + Position + Vector3.Up, Color.Blue);
        }

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