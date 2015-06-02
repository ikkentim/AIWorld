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
    ///     Adds some methods to the <see cref="Vector3" /> class.
    /// </summary>
    public static class Vector3Extension
    {
        /// <summary>
        ///     Rotates the specified <paramref name="point" /> about the specified <paramref name="origin" /> on the y-axis.
        /// </summary>
        /// <param name="point">The vector.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="rotation">The rotation.</param>
        /// <returns></returns>
        public static Vector3 RotateAboutOriginY(this Vector3 point, Vector3 origin, float rotation)
        {
            return Vector3.Transform(point - origin, Matrix.CreateRotationY(rotation)) + origin;
        }

        /// <summary>
        ///     Gets the y-angle of the specified <paramref name="point" />.
        /// </summary>
        /// <param name="point">The vector.</param>
        /// <returns>The y-angle</returns>
        public static float GetYAngle(this Vector3 point)
        {
            return (float) Math.Atan2(point.X, point.Z);
        }

        /// <summary>
        ///     Truncates the <paramref name="vector" />.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The truncated vector.</returns>
        public static Vector3 Truncate(this Vector3 vector, float limit)
        {
            return vector.LengthSquared() > limit*limit ? Vector3.Normalize(vector)*limit : vector;
        }

        /// <summary>
        ///     Gets the manhattan length of the specified <paramref name="vector" />.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <returns></returns>
        public static float ManhattanLength(this Vector3 vector)
        {
            return Math.Abs(vector.X) + Math.Abs(vector.Y) + Math.Abs(vector.Z);
        }

        public static Vector2 ToVector2XY(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
        public static Vector2 ToVector2XZ(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Z);
        }
        public static Vector3 ToVector3(this Vector2 vector)
        {
            return new Vector3(vector, 0);
        }
    }
}