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
using Microsoft.Xna.Framework;

namespace AIWorld.Steering
{
    public struct WeightedSteeringBehavior
    {
        public WeightedSteeringBehavior(ISteeringBehavior behavior, float weight)
            : this()
        {
            if (behavior == null) throw new ArgumentNullException("behavior");
            if (weight <= 0) throw new ArgumentException("weight must be greater than 0");

            Behavior = behavior;
            Weight = weight;
        }

        public ISteeringBehavior Behavior { get; set; }
        public float Weight { get; set; }

        public Vector3 Calculate()
        {
            return Behavior.Calculate()*Weight;
        }
    }
}