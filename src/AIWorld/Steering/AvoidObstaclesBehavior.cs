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
    public class AvoidObstaclesBehavior : ISteeringBehavior
    {
        //private const float MinimumDetectionBoxLength = 0.75f;
        private const float MinimumDetectionBoxLengthSizeMultiplier = 1.9f;
        //private const float AproxMaxObjectSize = 1.0f;
        private const float BreakingWeight = 0.005f * 0.001f;
        private readonly Agent _agent;
        private readonly IGameWorldService _gameWorldService;

        public AvoidObstaclesBehavior(Agent agent)
        {
            _agent = agent;
            _gameWorldService = agent.Game.Services.GetService<IGameWorldService>();
        }

        private float DetectionBoxLength
        {
            get
            {
                return _agent.Size*MinimumDetectionBoxLengthSizeMultiplier +
                       (_agent.Velocity.Length()/_agent.MaxSpeed)*_agent.Size*MinimumDetectionBoxLengthSizeMultiplier;
            }
        }

        #region Implementation of ISteeringBehavior

        public Vector3 Calculate(GameTime gameTime)
        {
            var bLength = DetectionBoxLength;

            var entities =
                _gameWorldService.Entities.Query(new AABB(_agent.Position, new Vector3(bLength + Entity.MaxSize)))
                    .Where(e => e != _agent && (e.Position - _agent.Position).Length() < e.Size + bLength);

            IEntity closest = null;
            var closestDistance = float.MaxValue;
            var localPositionOfClosestPoint = Vector3.Zero;

            foreach (var e in entities)
            {
                var localPoint = Transform.ToLocalSpace(_agent.Position, _agent.Heading, Vector3.Up, _agent.Side,
                    e.Position);
                if (localPoint.X > 0)
                {
                    var combinedSize = e.Size + _agent.Size;

                    if (Math.Abs(localPoint.Z) < combinedSize)
                    {
                        var sqrtpart = (float) Math.Sqrt(combinedSize*combinedSize - localPoint.Z*localPoint.Z);

                        var ip = sqrtpart <= localPoint.X ? localPoint.X - sqrtpart : localPoint.X + sqrtpart;

                        if (ip < closestDistance)
                        {
                            closestDistance = ip;
                            closest = e;
                            localPositionOfClosestPoint = localPoint;
                        }
                    }
                }
            }

            if (closest == null)
                return Vector3.Zero;

            var multiplier = 1 + (bLength - localPositionOfClosestPoint.X)/bLength;

            return
                Transform.VectorToWorldSpace(_agent.Heading, Vector3.Up, _agent.Side,
                    new Vector3((closest.Size - localPositionOfClosestPoint.X)*BreakingWeight, 0,
                        closest.Size - localPositionOfClosestPoint.Z*multiplier));
        }

        #endregion
    }
}