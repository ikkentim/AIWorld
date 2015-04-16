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

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld
{
    internal class RoadPlanesGenerator
    {
        public static IEnumerable<Plane> Generate(GraphicsDevice graphicsDevice, Texture2D texture, Vector3[] roadNodes)
        {
            Vector3 previousLeft = Vector3.Zero;
            Vector3 previousRight = Vector3.Zero;
            Vector3 previousAbsoluteLeft = Vector3.Zero;
            Vector3 previousAbsoluteRight = Vector3.Zero;
            for (int i = 0; i < roadNodes.Length; i++)
            {
                bool hasPrevious = i != 0;
                bool hasNext = i != roadNodes.Length - 1;

                if (!hasNext && !hasPrevious) break;

                Vector3 current = roadNodes[i];
                Vector3 next = hasNext ? roadNodes[i + 1] : Vector3.Zero;

                if (!hasPrevious)
                {
                    Vector3 offsetBetweenNextAndCurrent = next - current;
                    offsetBetweenNextAndCurrent.Normalize();
                    offsetBetweenNextAndCurrent /= 2;

                    previousLeft = offsetBetweenNextAndCurrent.RotateAboutOriginY(Vector3.Zero,
                        MathHelper.ToRadians(-90));
                    previousRight = offsetBetweenNextAndCurrent.RotateAboutOriginY(Vector3.Zero,
                        MathHelper.ToRadians(90));

                    previousAbsoluteLeft = current + previousLeft;
                    previousAbsoluteRight = current + previousRight;
                }
                else if (hasNext)
                {
                    Vector3 offsetBetweenNextAndCurrent = next - current;
                    offsetBetweenNextAndCurrent.Normalize();
                    offsetBetweenNextAndCurrent /= 2;
                    Vector3 left = offsetBetweenNextAndCurrent.RotateAboutOriginY(Vector3.Zero,
                        MathHelper.ToRadians(-90));
                    Vector3 right = offsetBetweenNextAndCurrent.RotateAboutOriginY(Vector3.Zero,
                        MathHelper.ToRadians(90));

                    Vector3 avgLeft = (previousLeft + left)/2;
                    Vector3 avgRight = (previousRight + right)/2;
                    avgLeft.Normalize();
                    avgRight.Normalize();
                    avgLeft /= 2;
                    avgRight /= 2;
                    yield return new Plane(graphicsDevice, previousAbsoluteLeft, previousAbsoluteRight,
                        previousAbsoluteLeft = current + avgLeft,
                        previousAbsoluteRight = current + avgRight,
                        PlaneRotation.None, texture);

                    previousLeft = left;
                    previousRight = right;
                }
                else
                {
                    yield return
                        new Plane(graphicsDevice, previousAbsoluteLeft, previousAbsoluteRight, current + previousLeft,
                            current + previousRight,
                            PlaneRotation.None, texture);
                }
            }
        }
    }
}