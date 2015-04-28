using System;
using System.Collections.Generic;
using System.Linq;
using AIWorld.Helpers;
using AIWorld.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld
{
    public class Road : DrawableGameComponent
    {
        private ICameraService _cameraService;
        public Vector3[] Nodes { get; private set; }

        public Vector3[] RightNodes { get; private set; }
        public Vector3[] LeftNodes { get; private set; }

        public QuadPlane[] QuadPlanes { get; private set; }

        public Road(Game game, Vector3[] nodes)
            : base(game)
        {
            if (game == null) throw new ArgumentNullException("game");
            if (nodes == null) throw new ArgumentNullException("nodes");
            if(nodes.Length < 2) throw new ArgumentException("nodes must contain 2 or more items");

            _cameraService = game.Services.GetService<ICameraService>();

            Nodes = nodes;
            LeftNodes = new Vector3[nodes.Length];
            RightNodes = new Vector3[nodes.Length];

            Vector3 previousLeft = Vector3.Zero;
            Vector3 previousRight = Vector3.Zero;
            for (int i = 0; i < nodes.Length; i++)
            {
                bool hasPrevious = i != 0;
                bool hasNext = i != nodes.Length - 1;

                if (!hasNext && !hasPrevious) break;

                Vector3 current = nodes[i];
                Vector3 next = hasNext ? nodes[i + 1] : Vector3.Zero;

                if (!hasPrevious)
                {
                    Vector3 offsetBetweenNextAndCurrent = next - current;
                    offsetBetweenNextAndCurrent.Normalize();
                    offsetBetweenNextAndCurrent /= 4;

                    previousLeft = offsetBetweenNextAndCurrent.RotateAboutOriginY(Vector3.Zero,
                        MathHelper.ToRadians(-90));
                    previousRight = offsetBetweenNextAndCurrent.RotateAboutOriginY(Vector3.Zero,
                        MathHelper.ToRadians(90));

                    LeftNodes[nodes.Length - i - 1] = current + previousRight;
                    RightNodes[i] = current + previousLeft;

                }
                else if (hasNext)
                {
                    Vector3 offsetBetweenNextAndCurrent = next - current;
                    offsetBetweenNextAndCurrent.Normalize();
                    offsetBetweenNextAndCurrent /= 4;

                    Vector3 left = offsetBetweenNextAndCurrent.RotateAboutOriginY(Vector3.Zero,
                        MathHelper.ToRadians(-90));
                    Vector3 right = offsetBetweenNextAndCurrent.RotateAboutOriginY(Vector3.Zero,
                        MathHelper.ToRadians(90));

                    Vector3 avgLeft = (previousLeft + left) / 2;
                    Vector3 avgRight = (previousRight + right) / 2;
                    avgLeft.Normalize();
                    avgRight.Normalize();
                    avgLeft /= 4;
                    avgRight /= 4;

                    LeftNodes[nodes.Length - i - 1] = current + avgRight;
                    RightNodes[i] = current + avgLeft;

                    previousLeft = left;
                    previousRight = right;
                }
                else
                {
                    LeftNodes[nodes.Length - i - 1] = current + previousRight;
                    RightNodes[i] = current + previousLeft;
                }
            }


            QuadPlanes =
                RoadPlanesGenerator.Generate(game.GraphicsDevice, game.Content.Load<Texture2D>(@"textures/road"), nodes)
                    .ToArray();
        }

        #region Overrides of DrawableGameComponent

        public override void Draw(GameTime gameTime)
        {
            foreach (QuadPlane t in QuadPlanes)
                t.Render(GraphicsDevice, Matrix.Identity, _cameraService.View, _cameraService.Projection);

            base.Draw(gameTime);
        }

        #endregion
    }
}