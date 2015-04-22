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

namespace AIWorld.Helpers
{
    public static class Vector2Extension
    {
        public static Vector3 RotateAboutOriginY(this Vector3 point, Vector3 origin, float rotation)
        {
            return Vector3.Transform(point - origin, Matrix.CreateRotationY(rotation)) + origin;
        }

        public static float GetYAngle(this Vector3 point)
        {
            return (float) Math.Atan2(point.X, point.Z);
        }

        public static Vector3 Truncate(this Vector3 point, float limit)
        {
            return point.LengthSquared() > limit*limit ? Vector3.Normalize(point)*limit : point;
        }
    }
}