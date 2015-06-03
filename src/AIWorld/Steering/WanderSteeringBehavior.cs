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
using AIWorld.Entities;
using AIWorld.Helpers;
using Microsoft.Xna.Framework;

namespace AIWorld.Steering
{
    public class WanderSteeringBehavior : ISteeringBehavior
    {
        private static readonly Random Random = new Random();
        private Vector3 _wanderTarget;

        public WanderSteeringBehavior(Agent agent)
        {
            Agent = agent;

            var theta = (float) Random.NextDouble()*(float) Math.PI*2;
            _wanderTarget = new Vector3(Radius*(float) Math.Cos(theta), 0,
                Radius*(float) Math.Sin(theta));
        }

        [SteeringBehaviorArgument(0)]
        public float Jitter { get; private set; }

        [SteeringBehaviorArgument(1)]
        public float Radius { get; private set; }

        [SteeringBehaviorArgument(2)]
        public float Distance { get; private set; }

        public Agent Agent { get; private set; }

        #region Implementation of ISteeringBehavior

        public Vector3 Calculate(GameTime gameTime)
        {
            var jitter = Jitter*((float) gameTime.ElapsedGameTime.TotalSeconds);

            var add = new Vector3((float) ((Random.NextDouble()*2) - 1)*jitter, 0,
                (float) ((Random.NextDouble()*2) - 1)*jitter);
            _wanderTarget += add;

            _wanderTarget = Vector3.Normalize(_wanderTarget)*Radius;

            return
                Transform.PointToWorldSpace(Agent.Position, Agent.Heading, Vector3.Up, Agent.Side,
                    _wanderTarget + new Vector3(Distance, 0, 0)) - Agent.Position;
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return "Wander";
        }

        #endregion
    }
}