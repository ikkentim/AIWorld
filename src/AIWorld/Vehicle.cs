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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld
{
    internal class Vehicle : IMovingEntity
    {
        private readonly Road _r;

        #region Implementation of IEntity

        private Model model;
        private int targetnode;

        Vector3 Seek(Vector3 target)
        {
            var desiredVelocity = Vector3.Normalize(target - Position) * MaxSpeed;
            return desiredVelocity - Velocity; 
        }
        Vector3 Arrive(Vector3 target, int decel)
        {
            var toTarget = target - Position;
            var distance = toTarget.Length();

            if (distance > 0.00001)
            {
                const float decelerationTweaker = 0.3f;

                float speed = distance/(decel*decelerationTweaker);
                speed = Math.Min(speed, MaxSpeed);
                var desiredVelocity = toTarget*speed/distance;

                return desiredVelocity - Velocity;
            }

            return Vector3.Zero;
        }
        Vector3 CalculateSteeringForce()
        {
            var target = _r.Nodes[targetnode];

            var sk = Arrive(target, 3);

            return sk.Truncate(MaxForce);
        }

        public void Update(GameWorld world, GameTime gameTime)
        {
            if ((Position - _r.Nodes[targetnode]).Length() < 0.25)
            {
                targetnode++;
                targetnode %= _r.Nodes.Length;
            }
            // update pos
            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            var steeringForce = CalculateSteeringForce();

            Vector3 acceleration = steeringForce/Mass;
            Velocity += acceleration*deltaTime;
     
            Velocity = Velocity.Truncate(MaxSpeed);
      
            Position += Velocity * deltaTime;

            if (Velocity.LengthSquared() > 0.00001)
            {
                Heading = Vector3.Normalize(Velocity);
                Side = Heading.RotateAboutOriginY(Vector3.Zero, MathHelper.ToRadians(90));
            }
        }

        public void Render(GraphicsDevice graphicsDevice, Matrix view, Matrix projection, GameTime gameTime)
        {
//            var vertices = new[]
//            {
//                new VertexPositionColor(Position - Heading/4 - Side/10, Color.Blue),
//                new VertexPositionColor(Position + Vector3.Up / 10, Color.Red),
//                new VertexPositionColor(Position - Heading/4 + Side/10, Color.Blue)
//            };
//            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, vertices, 0, 2);

            var transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {


                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateRotationY(Heading.GetYAngle()) *
                                   Matrix.CreateTranslation(Position);
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }

        public Vector3 Position { get; private set; }

        #endregion

        public Vehicle(Vector3 position, ContentManager content, Road r)
        {
            _r = r;
            model = content.Load<Model>("models/car");

            Position = position;
            MaxTurnRate = 2;
            MaxForce = 4.0f;
            MaxSpeed = 150;
            Mass = 1;
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