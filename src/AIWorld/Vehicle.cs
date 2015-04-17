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
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld
{
    internal class Vehicle : IMovingEntity
    {
        private readonly Road _r;

        #region Implementation of IEntity

        private readonly AudioEmitter _audioEmitter;
        private readonly AudioListener _audioListener;
        private readonly SoundEffectInstance _soundEffectInstance;
        private readonly Model _model;
        private int _targetnode;

        public void Update(GameWorld world, Matrix view, Matrix projection, GameTime gameTime)
        {
            var cpos = Matrix.Invert(view).Translation;
            _audioListener.Position = cpos;
            _audioListener.Forward = new Vector3(view.M31, view.M32, view.M33); // Think this is the right col?
            _audioEmitter.Forward = Heading;
            _audioEmitter.Position = Position;
            _audioEmitter.Velocity = Velocity;
            _audioEmitter.DopplerScale = Math.Max(1.0f, ((Vector3.Normalize(Position - cpos) - Velocity).Length())); // Just testing it out
            Debug.WriteLine(_audioEmitter.DopplerScale);
            _soundEffectInstance.Apply3D(_audioListener, _audioEmitter);

            if ((Position - _r.RightNodes[_targetnode]).Length() < 0.4f)
            {
                _targetnode++;
                _targetnode %= _r.Nodes.Length;
            }
            // update pos
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
        }

        public Vector3 Position { get; private set; }

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

        private Vector3 CalculateSteeringForce()
        {
            Vector3 target = _r.RightNodes[_targetnode];

            Vector3 sk = Seek(target);

            return sk.Truncate(MaxForce);
        }

        #endregion

        public Vehicle(Vector3 position, SoundEffect engine, ContentManager content, Road r)
        {
            _r = r;
            _model = content.Load<Model>("models/car");

            Position = position;
            MaxTurnRate = 2;
            MaxForce = 20000.0f;
            MaxSpeed = 10;
            Mass = 0.4f;

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