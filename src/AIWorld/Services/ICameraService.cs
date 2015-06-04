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

using AIWorld.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace AIWorld.Services
{
    public interface ICameraService : IGameComponent
    {
        Matrix View { get; }
        Matrix Projection { get; }
        Vector3 TargetPosition { get; set; }
        AudioListener AudioListener { get; }
        float Zoom { get; }
        float Rotation { get; }
        void SetTarget(IEntity target);
        void SetTarget(Vector3 target);
        void AddVelocity(Vector3 acceleration);
        void Move(float deltaRotation, float deltaZoom);
    }
}