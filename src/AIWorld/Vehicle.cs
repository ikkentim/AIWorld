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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld
{
    internal class Vehicle : IMovingEntity
    {
        #region Implementation of IEntity

        public void Update(GameWorld world, GameTime gameTime)
        {
            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            var steeringForce = new Vector3(1000, 0, 100);

            Vector3 acceleration = steeringForce/Mass;
            Vector3 velocity = acceleration*deltaTime;
            float velocityLength = velocity.Length();
            if (velocityLength > MaxSpeed)
            {
                velocity.Normalize();
                velocity *= MaxSpeed;
            }
            Position += velocity*deltaTime;

            if (velocityLength > 0.00000001)
            {
                Heading = velocity;
                Heading.Normalize();
                //Side = Heading.Perp();
            }
        }

        public void Render(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            var vertices = new[]
            {new VertexPositionColor(Position, Color.Red), new VertexPositionColor(Position + Vector3.Up, Color.Red)};
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        public Vector3 Position { get; private set; }

        #endregion

        public Vehicle(Vector3 position)
        {
            Position = position;
            MaxTurnRate = 45;
            MaxForce = 2;
            MaxSpeed = 100;
            Mass = 4;
        }

        #region Implementation of IMovingEntity

        public Vector3 Velocity { get; private set; }
        public float Mass { get; private set; }
        public Vector3 Heading { get; private set; }
        public float MaxSpeed { get; private set; }
        public float MaxForce { get; private set; }
        public float MaxTurnRate { get; private set; }

        #endregion
    }
}