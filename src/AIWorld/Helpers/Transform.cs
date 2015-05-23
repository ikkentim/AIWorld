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
    /// <summary>
    /// Contains various transformation methods.
    /// </summary>
    public static class Transform
    {
        /// <summary>
        /// Converts point to local space.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="heading">The heading.</param>
        /// <param name="up">Up.</param>
        /// <param name="side">The side.</param>
        /// <param name="point">The point.</param>
        /// <returns>Local point.</returns>
        public static Vector3 ToLocalSpace(Vector3 position, Vector3 heading, Vector3 up, Vector3 side, Vector3 point)
        {
            var tx = -Vector3.Dot(position, heading);
            var ty = -Vector3.Dot(position, up);
            var tz = -Vector3.Dot(position, side);

            return Vector3.Transform(point,
                new Matrix(heading.X, 0, side.X, 0, heading.Y, 1, side.Y, 0, heading.Z, 0, side.Z, 0, tx, ty, tz, 0));
        }

        /// <summary>
        /// Converts vector to world space.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="up">Up.</param>
        /// <param name="side">The side.</param>
        /// <param name="vector">The vector.</param>
        /// <returns>Vector in world space.</returns>
        public static Vector3 VectorToWorldSpace(Vector3 heading, Vector3 up, Vector3 side, Vector3 vector)
        {
            return Vector3.Transform(vector,
                Matrix.Identity*
                new Matrix(heading.X, up.X, side.X, 0, heading.Y, up.Y, side.Y, 0, heading.Z, up.Z, side.Z, 0, 0, 0, 0,
                    0));
        }

        /// <summary>
        /// Converts point to world space.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="heading">The heading.</param>
        /// <param name="up">Up.</param>
        /// <param name="side">The side.</param>
        /// <param name="point">The point.</param>
        /// <returns>Point in world space.</returns>
        public static Vector3 PointToWorldSpace(Vector3 position, Vector3 heading, Vector3 up, Vector3 side,
            Vector3 point)
        {
            var m1 = new Matrix(heading.X, heading.Y, heading.Z, 0, up.X, up.Y, up.Z, 0, side.X, side.Y, side.Z, 0,
                0, 0, 0, 1);
            var m2 = new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, position.X, position.Y, position.Z, 1);
            var m3 = m1*m2;
            return Vector3.Transform(point, m3);
        }
    }
}