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
using Microsoft.Xna.Framework;

namespace AIWorld.Steering
{
    public class ExploreSteeringBehavior : ISteeringBehavior
    {
        private ITargetedSteeringBehavior _behavior;
        private Vector3 _bottomRight;
        private bool _isTargetBottom;
        private Vector3 _topLeft;

        public ExploreSteeringBehavior(Agent agent, Vector3 size)
        {
            Agent = agent;
            _topLeft = agent.Position - size;
            _bottomRight = agent.Position + size;
        }

        public Agent Agent { get; private set; }

        #region Implementation of ISteeringBehavior

        public Vector3 Calculate()
        {
            var target = _isTargetBottom ? new Vector3(_topLeft.X, 0, _bottomRight.Z) : _topLeft;

            //Debug.WriteLine("Current target is {0}", target);
            if (_behavior is ArriveSteeringBehavior)
            {
                return _behavior.Calculate();
            }

            if (Agent.IsInTargetRangeOfPoint(target))
            {
                Debug.WriteLine("Reached target");
                _isTargetBottom = !_isTargetBottom;
                _behavior = null;
                _topLeft.X += Math.Max(1, Agent.TargetRange * 2);
                target = _isTargetBottom ? new Vector3(_topLeft.X, 0, _bottomRight.Z) : _topLeft;
            }

            if (_behavior == null)
            {
                Debug.WriteLine("Target wideness = {0}", Math.Abs(_bottomRight.X - _topLeft.X));
                if (Math.Abs(_bottomRight.X - _topLeft.X) < Math.Max(1, Agent.TargetRange*2))
                {
                    Debug.WriteLine("Create arrive");
                    _behavior = new ArriveSteeringBehavior(Agent, target);
                }
                else
                {
                    Debug.WriteLine("Create seek");
                    _behavior = new SeekSteeringBehavior(Agent, target);
                }
            }

            return _behavior.Calculate();
        }

        #endregion
    }
}