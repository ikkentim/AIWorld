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
using System.Linq;
using AIWorld.Entities;
using AIWorld.Services;
using Microsoft.Xna.Framework;

namespace AIWorld.Steering
{
    internal class PursuitSteeringBehavior : ISteeringBehavior
    {
        private readonly Agent _agent;
        private readonly IGameWorldService _gameWorldService;
        private readonly SeekSteeringBehavior _seek;
        private IMovingEntity _evader;

        public PursuitSteeringBehavior(Agent agent)
        {
            if (agent == null) throw new ArgumentNullException("agent");
            _agent = agent;
            _seek = new SeekSteeringBehavior(agent);
            _gameWorldService = agent.Game.Services.GetService<IGameWorldService>();
        }

        [SteeringBehaviorArgument]
        public int EvaderId
        {
            get { return _evader == null ? -1 : _evader.Id; }
            set { _evader = _gameWorldService.Entities.OfType<IMovingEntity>().FirstOrDefault(e => e.Id == value); }
        }

        #region Implementation of ISteeringBehavior

        public Vector3 Calculate(GameTime gameTime)
        {
            if (_evader == null) return Vector3.Zero;

            var toEvader = _evader.Position - _agent.Position;

            var relativeheading = Vector3.Dot(_agent.Heading, _evader.Heading);

            if (Vector3.Dot(toEvader, _agent.Heading) > 0 && relativeheading < -0.95f) //18 degrees
            {
                _seek.Target = _evader.Position;
                return _seek.Calculate(gameTime);
            }

            var lookAheadTime = toEvader.Length()/(_agent.MaxSpeed + _evader.Velocity.Length());

            _seek.Target = _evader.Position + _evader.Velocity*lookAheadTime;
            return _seek.Calculate(gameTime);
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return "Pursuit";
        }

        #endregion
    }
}