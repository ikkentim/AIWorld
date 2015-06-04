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
    public abstract class SeparationSteeringBehavior : ISteeringBehavior
    {
        private readonly Agent _agent;
        private readonly IGameWorldService _gameWorldService;
        private Vector3 _calculated;
        private float _range;
        private float _rangeSquared;

        protected SeparationSteeringBehavior(Agent agent)
        {
            if (agent == null) throw new ArgumentNullException("agent");
            _agent = agent;
            _gameWorldService = agent.Game.Services.GetService<IGameWorldService>();
        }

        [SteeringBehaviorArgument(0)]
        public float Range
        {
            get { return _range; }
            set
            {
                _range = value;
                _rangeSquared = value*value;
            }
        }

        [SteeringBehaviorArgument(1)]
        public string Key { get; set; }

        public object KeyValue { get; protected set; }

        #region Implementation of ISteeringBehavior

        public Vector3 Calculate(GameTime gameTime)
        {
            if (KeyValue == null) return Vector3.Zero;

            return _calculated =
                _gameWorldService.Entities.Query(new AABB(_agent.Position, new Vector3(Range)))
                    .OfType<Agent>()
                    .Where(a => a != _agent)
                    .Where(a => KeyValue.Equals(a.GetVarObject(Key)))
                    .Where(a => Vector3.DistanceSquared(_agent.Position, a.Position) < _rangeSquared)
                    .Select(agent => _agent.Position - agent.Position)
                    .Aggregate(Vector3.Zero, (current, toAgent) =>
                    {
                        var length = toAgent.Length();
                        return length == 0 ? Vector3.Zero : current + Vector3.Normalize(toAgent)/length;
                    });
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
            return string.Format("Separation ({0},{1})", _calculated.X, _calculated.Z);
        }

        #endregion
    }
}