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
using AIWorld.Helpers;
using AIWorld.Services;
using Microsoft.Xna.Framework;

namespace AIWorld.Steering
{
    internal class OffsetPursuitSteeringBehavior : ISteeringBehavior
    {
        private readonly Agent _agent;
        private readonly ArriveSteeringBehavior _arrive;
        private readonly IGameWorldService _gameWorldService;
        private IMovingEntity _leader;

        public OffsetPursuitSteeringBehavior(Agent agent)
        {
            if (agent == null) throw new ArgumentNullException("agent");

            _arrive = new ArriveSteeringBehavior(agent);
            _gameWorldService = agent.Game.Services.GetService<IGameWorldService>();
            _agent = agent;
        }

        [SteeringBehaviorArgument(0)]
        public int LeaderId
        {
            get { return _leader == null ? -1 : _leader.Id; }
            set { _leader = _gameWorldService.Entities.OfType<IMovingEntity>().FirstOrDefault(e => e.Id == value); }
        }

        [SteeringBehaviorArgument(1)]
        public Vector3 Offset { get; set; }

        #region Implementation of ISteeringBehavior

        public Vector3 Calculate(GameTime gameTime)
        {
            if (_leader == null)
                return Vector3.Zero;

            var worldOffsetPos = Transform.PointToWorldSpace(_leader.Position, _leader.Heading, Vector3.Up, _leader.Side,
                Offset);

            var toOffset = worldOffsetPos - _agent.Position;

            var lookAheadTime = toOffset.Length()/(_agent.MaxSpeed + _leader.Velocity.Length());

            _arrive.Target = worldOffsetPos + _leader.Velocity*lookAheadTime;

            return _arrive.Calculate(gameTime);
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
            return "OffsetPursuit";
        }

        #endregion
    }
}