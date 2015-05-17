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
using AIWorld.Entities;
using Microsoft.Xna.Framework;

namespace AIWorld.Steering
{
    public class ArriveSteeringBehavior : ITargetedSteeringBehavior
    {
        private const float ArriveDecelerationTweaker = 1.3f;

        public Agent Agent { get; private set; }
        public Vector3 Target { get; set; }

        public ArriveSteeringBehavior(Agent agent, Vector3 target)
        {
            Agent = agent;
            Target = target;
        }

        #region Implementation of ISteeringBehavior

        public Vector3 Calculate(GameTime gameTime)
        {
            var toTarget = Target - Agent.Position;
            var distance = toTarget.Length();

            if (distance > 0.00001)
            {
                var speed = distance/(ArriveDecelerationTweaker);
                speed = Math.Min(speed, Agent.MaxSpeed);
                var desiredVelocity = toTarget*speed/distance;

                return desiredVelocity - Agent.Velocity;
            }

            return Vector3.Zero;
        }

        #endregion
    }
}