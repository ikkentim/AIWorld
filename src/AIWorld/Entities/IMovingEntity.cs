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

using Microsoft.Xna.Framework;

namespace AIWorld.Entities
{
    public interface IMovingEntity : IEntity
    {
        Vector3 Velocity { get; }
        float Mass { get; }
        Vector3 Heading { get; }
        Vector3 Side { get; }
        float MaxSpeed { get; }
        float MaxForce { get; }
    }
}