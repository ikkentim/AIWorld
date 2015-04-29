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

namespace AIWorld.Helpers
{
    public static class Transform
    {
        public static Vector3 ToLocalSpace(Vector3 position, Vector3 heading, Vector3 up, Vector3 side, Vector3 point)
        {
            var tx = -Vector3.Dot(position, heading);
            var ty = -Vector3.Dot(position, up);
            var tz = -Vector3.Dot(position, side);

            return Vector3.Transform(point,
                new Matrix(heading.X, 0, side.X, 0, heading.Y, 1, side.Y, 0, heading.Z, 0, side.Z, 0, tx, ty, tz, 0));
        }

        public static Vector3 VectorToWorldSpace(Vector3 heading, Vector3 up, Vector3 side, Vector3 vec)
        {
            return Vector3.Transform(vec,
                Matrix.Identity*
                new Matrix(heading.X, up.X, side.X, 0, heading.Y, up.Y, side.Y, 0, heading.Z, up.Z, side.Z, 0, 0, 0, 0,
                    0));
        }
    }
}