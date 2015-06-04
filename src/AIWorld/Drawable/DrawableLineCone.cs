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
using AIWorld.Helpers;
using AIWorld.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Drawable
{
    public class DrawableLineCone : DrawableLineCylinder
    {
        public DrawableLineCone(ICameraService cameraService, GraphicsDevice graphicsDevice, Vector3 position,
            Vector3 direction, float length, float radius, Color color, Color secondColor)
            : base(cameraService, graphicsDevice, position, direction, length, radius, color, secondColor)
        {
        }

        #region Overrides of DrawableLineCylinder

        protected override void CreateCylinder()
        {
            var verts = Math.Max(RadiusMultiplier, (int) (RadiusMultiplier*Radius));

            var offs = (float) (2*Math.PI)/(verts);

            Vertices = new VertexPositionColor[verts*2*2];
            Count = verts*2;

            var heading = Direction;
            var side =
                Vector3.Normalize(heading.X == 0 && heading.Z == 0
                    ? Vector3.UnitX
                    : -new Vector3(heading.Z, 0, heading.X));
            var up = Vector3.Cross(side, heading);

            for (var i = 0; i < verts; i++)
            {
                var x1 = (float) Math.Cos(i*offs)*Radius;
                var z1 = (float) Math.Sin(i*offs)*Radius;

                var x2 = (float) Math.Cos(i*offs + offs)*Radius;
                var z2 = (float) Math.Sin(i*offs + offs)*Radius;

                Vertices[i*4 + 0] =
                    new VertexPositionColor(
                        Transform.PointToWorldSpace(Position, heading, up, side, new Vector3(0, x1, z1)), Color);
                Vertices[i*4 + 1] =
                    new VertexPositionColor(
                        Transform.PointToWorldSpace(Position, heading, up, side, new Vector3(Length, 0, 0)),
                        SecondColor);
                Vertices[i*4 + 2] =
                    new VertexPositionColor(
                        Transform.PointToWorldSpace(Position, heading, up, side, new Vector3(0, x1, z1)), Color);
                Vertices[i*4 + 3] =
                    new VertexPositionColor(
                        Transform.PointToWorldSpace(Position, heading, up, side, new Vector3(0, x2, z2)), Color);
            }
        }

        #endregion
    }
}