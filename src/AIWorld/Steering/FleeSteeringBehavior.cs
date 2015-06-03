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
using AIWorld.Helpers;
using Microsoft.Xna.Framework;

namespace AIWorld.Steering
{
    public class FleeSteeringBehavior : ISteeringBehavior
    {
        private readonly Agent _agent;

        public FleeSteeringBehavior(Agent agent)
        {
            if (agent == null) throw new ArgumentNullException("agent");
            _agent = agent;
        }

        [SteeringBehaviorArgument]
        public Vector3 Target { get; set; }

        #region Implementation of ISteeringBehavior

        public virtual Vector3 Calculate(GameTime gameTime)
        {
            return (_agent.Position - Target).Truncate(_agent.MaxSpeed) - _agent.Velocity;
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
            return "Flee";
        }

        #endregion
    }
}