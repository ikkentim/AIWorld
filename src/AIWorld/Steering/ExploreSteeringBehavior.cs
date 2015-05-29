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
    public class ExploreSteeringBehavior : ISteeringBehavior
    {
        private ITargetedSteeringBehavior _behavior;
        private Vector3 _bottomRight;
        private Vector3 _center;
        private bool _isTargetBottom;
        private Vector3 _topLeft;

        public ExploreSteeringBehavior(Agent agent)
        {
            Agent = agent;
            _topLeft = _bottomRight = _center = agent.Position;
        }

        [SteeringBehaviorArgument(0)]
        public float Width
        {
            get { return _bottomRight.X - _topLeft.X; }
            set
            {
                _bottomRight.X = _center.X + value/2;
                _topLeft.X = _center.X - value/2;
            }
        }

        [SteeringBehaviorArgument(1)]
        public float Height
        {
            get { return _bottomRight.Y - _topLeft.Y; }
            set
            {
                _bottomRight.Y = _center.Y + value/2;
                _topLeft.Y = _center.Y - value/2;
            }
        }

        public Agent Agent { get; private set; }

        #region Implementation of ISteeringBehavior

        public Vector3 Calculate(GameTime gameTime)
        {
            var target = _isTargetBottom ? new Vector3(_topLeft.X, 0, _bottomRight.Z) : _topLeft;

            if (_behavior is ArriveSteeringBehavior) return _behavior.Calculate(gameTime);

            if (Agent.IsInTargetRangeOfPoint(target))
            {
                _isTargetBottom = !_isTargetBottom;
                _behavior = null;
                _topLeft.X += Math.Max(1, Agent.TargetRange*2);
                target = _isTargetBottom ? new Vector3(_topLeft.X, 0, _bottomRight.Z) : _topLeft;
            }

            if (_behavior != null) return _behavior.Calculate(gameTime);

            _behavior = ((Math.Abs(_bottomRight.X - _topLeft.X) < Math.Max(1, Agent.TargetRange*2))
                ? (ITargetedSteeringBehavior) new ArriveSteeringBehavior(Agent) {Target = target}
                : new SeekSteeringBehavior(Agent) {Target = target});

            return _behavior.Calculate(gameTime);
        }

        #endregion
    }
}