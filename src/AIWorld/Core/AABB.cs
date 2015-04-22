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

namespace AIWorld
{
    public struct AABB
    {
        public AABB(Vector3 center, Vector3 halfDimension) : this()
        {
            Center = center;
            HalfDimension = halfDimension;
        }

        public Vector3 Center { get; private set; }

        public Vector3 HalfDimension { get; private set; }

        public bool ContainsPoint(Vector3 point)
        {
            if (Center.X - HalfDimension.X > point.X)
                return false;

            if (Center.X + HalfDimension.X < point.X)
                return false;

            if (Center.Y - HalfDimension.Y > point.Y)
                return false;

            if (Center.Y + HalfDimension.Y < point.Y)
                return false;

            if (Center.Z - HalfDimension.Z > point.Z)
                return false;

            if (Center.Z + HalfDimension.Z < point.Z)
                return false;

            return true;
        }

        public bool IntersectsWith(AABB other)
        {
            if (Center.X - HalfDimension.X > other.Center.X + other.HalfDimension.X)
                return false;

            if (Center.X + HalfDimension.X < other.Center.X - other.HalfDimension.X)
                return false;

            if (Center.Y - HalfDimension.Y > other.Center.Y + other.HalfDimension.Y)
                return false;

            if (Center.Y + HalfDimension.Y < other.Center.Y - other.HalfDimension.Y)
                return false;

            if (Center.Z - HalfDimension.Z > other.Center.Z + other.HalfDimension.Z)
                return false;

            if (Center.Z + HalfDimension.Z < other.Center.Z - other.HalfDimension.Z)
                return false;

            return true;
        }

        #region Overrides of ValueType

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            return string.Format("<{0}, {1}>", Center, HalfDimension);
        }

        #endregion
    }
}