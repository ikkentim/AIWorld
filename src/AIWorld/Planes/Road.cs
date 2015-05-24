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
using System.Collections.Generic;
using AIWorld.Core;
using AIWorld.Helpers;
using AIWorld.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Planes
{
    public static class Road
    {
        public static void GenerateRoad(Game game, Graph graph, Vector3[] nodes)
        {
            var world = game.Services.GetService<IGameWorldService>();
            if (game == null) throw new ArgumentNullException("game");
            if (graph == null) throw new ArgumentNullException("graph");
            if (nodes == null) throw new ArgumentNullException("nodes");
            if (nodes.Length < 2) throw new ArgumentException("nodes must contain 2 or more items");

            var leftNodes = new Vector3[nodes.Length];
            var rightNodes = new Vector3[nodes.Length];

            var previousLeft = Vector3.Zero;
            var previousRight = Vector3.Zero;
            for (var i = 0; i < nodes.Length; i++)
            {
                var hasPrevious = i != 0;
                var hasNext = i != nodes.Length - 1;

                if (!hasNext && !hasPrevious) break;

                var current = nodes[i];
                var next = hasNext ? nodes[i + 1] : Vector3.Zero;

                if (!hasPrevious)
                {
                    var offsetBetweenNextAndCurrent = next - current;
                    offsetBetweenNextAndCurrent.Normalize();
                    offsetBetweenNextAndCurrent /= 4;

                    previousLeft = offsetBetweenNextAndCurrent.RotateAboutOriginY(Vector3.Zero,
                        MathHelper.ToRadians(-90));
                    previousRight = offsetBetweenNextAndCurrent.RotateAboutOriginY(Vector3.Zero,
                        MathHelper.ToRadians(90));

                    leftNodes[nodes.Length - i - 1] = current + previousRight;
                    rightNodes[i] = current + previousLeft;
                }
                else if (hasNext)
                {
                    var offsetBetweenNextAndCurrent = next - current;
                    offsetBetweenNextAndCurrent.Normalize();
                    offsetBetweenNextAndCurrent /= 4;

                    var left = offsetBetweenNextAndCurrent.RotateAboutOriginY(Vector3.Zero,
                        MathHelper.ToRadians(-90));
                    var right = offsetBetweenNextAndCurrent.RotateAboutOriginY(Vector3.Zero,
                        MathHelper.ToRadians(90));

                    var avgLeft = (previousLeft + left)/2;
                    var avgRight = (previousRight + right)/2;
                    avgLeft.Normalize();
                    avgRight.Normalize();
                    avgLeft /= 4;
                    avgRight /= 4;

                    leftNodes[nodes.Length - i - 1] = current + avgRight;
                    rightNodes[i] = current + avgLeft;

                    previousLeft = left;
                    previousRight = right;
                }
                else
                {
                    leftNodes[nodes.Length - i - 1] = current + previousRight;
                    rightNodes[i] = current + previousLeft;
                }
            }

            for (var i = 0; i < nodes.Length; i++)
            {
                if (i > 0)
                {
                    graph.Add(leftNodes[i - 1], leftNodes[i]);
                    graph.Add(rightNodes[i - 1], rightNodes[i]);
                }

                graph.Add(leftNodes[i], rightNodes[nodes.Length - 1 - i]);
                graph.Add(rightNodes[nodes.Length - 1 - i], leftNodes[i]);
            }

            foreach (
                var plane in
                    GeneratePlanes(game, game.Content.Load<Texture2D>(@"textures/road"),
                        nodes))
                world.Add(plane);
        }

        private static IEnumerable<QuadPlane> GeneratePlanes(Game game, Texture2D texture, Vector3[] roadNodes)
        {
            var previousLeft = Vector3.Zero;
            var previousRight = Vector3.Zero;
            var previousAbsoluteLeft = Vector3.Zero;
            var previousAbsoluteRight = Vector3.Zero;
            for (var i = 0; i < roadNodes.Length; i++)
            {
                var hasPrevious = i != 0;
                var hasNext = i != roadNodes.Length - 1;

                if (!hasNext && !hasPrevious) break;

                var current = roadNodes[i];
                var next = hasNext ? roadNodes[i + 1] : Vector3.Zero;

                if (!hasPrevious)
                {
                    var offsetBetweenNextAndCurrent = next - current;
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
                    var offsetBetweenNextAndCurrent = next - current;
                    offsetBetweenNextAndCurrent.Normalize();
                    offsetBetweenNextAndCurrent /= 2;
                    var left = offsetBetweenNextAndCurrent.RotateAboutOriginY(Vector3.Zero,
                        MathHelper.ToRadians(-90));
                    var right = offsetBetweenNextAndCurrent.RotateAboutOriginY(Vector3.Zero,
                        MathHelper.ToRadians(90));

                    var avgLeft = (previousLeft + left)/2;
                    var avgRight = (previousRight + right)/2;
                    avgLeft.Normalize();
                    avgRight.Normalize();
                    avgLeft /= 2;
                    avgRight /= 2;
                    yield return new QuadPlane(game, previousAbsoluteLeft, previousAbsoluteRight,
                        previousAbsoluteLeft = current + avgLeft,
                        previousAbsoluteRight = current + avgRight,
                        PlaneRotation.None, texture);

                    previousLeft = left;
                    previousRight = right;
                }
                else
                {
                    yield return
                        new QuadPlane(game, previousAbsoluteLeft, previousAbsoluteRight, current + previousLeft,
                            current + previousRight,
                            PlaneRotation.None, texture);
                }
            }
        }
    }
}