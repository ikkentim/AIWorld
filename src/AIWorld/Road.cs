using System;
using System.Collections.Generic;
using AIWorld.Helpers;
using AIWorld.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld
{
    public static class Road
    {
        public static void GenerateRoad(Game game, Vector3[] nodes)
        {
            var world = game.Services.GetService<IGameWorldService>();
            if (game == null) throw new ArgumentNullException("game");
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
                    world.Graph.Add(leftNodes[i - 1], leftNodes[i]);
                    world.Graph.Add(rightNodes[i - 1], rightNodes[i]);
                }

                world.Graph.Add(leftNodes[i], rightNodes[nodes.Length - 1 - i]);
                world.Graph.Add(rightNodes[nodes.Length - 1 - i], leftNodes[i]);
            }

            foreach (
                var plane in
                    GeneratePlanes(game, game.Content.Load<Texture2D>(@"textures/road"),
                        nodes))
                world.Add(plane);
        }

        private static IEnumerable<QuadPlane> GeneratePlanes(Game game, Texture2D texture, Vector3[] roadNodes)
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

                    Vector3 avgLeft = (previousLeft + left) / 2;
                    Vector3 avgRight = (previousRight + right) / 2;
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