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
    public class DrawableLineCylinder : IDrawable3D, IDrawableHasPosition, IDrawableHasColor, IDrawableHasSecondColor,
        IDrawableHasLength, IDrawableHasRadius
    {
        protected const int RadiusMultiplier = 16;
        private readonly BasicEffect _basicEffect;
        private readonly ICameraService _cameraService;
        private Color _color;
        private Vector3 _direction;
        private float _length;
        private Vector3 _position;
        private float _radius;
        private Color _secondColor;

        public DrawableLineCylinder(ICameraService cameraService, GraphicsDevice graphicsDevice, Vector3 position,
            Vector3 direction, float length, float radius, Color color, Color secondColor)
        {
            _position = position;
            _direction = direction.X == 0 && direction.Y == 0 && direction.Z == 0
                ? Vector3.Up
                : Vector3.Normalize(direction);
            _length = length;
            _radius = radius;
            _color = color;
            _secondColor = secondColor;

            _cameraService = cameraService;

            CreateCylinder();

            _basicEffect = new BasicEffect(graphicsDevice);
        }

        protected VertexPositionColor[] Vertices { get; set; }
        protected int Count { get; set; }

        public Vector3 Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                CreateCylinder();
            }
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                CreateCylinder();
            }
        }

        public float Length
        {
            get { return _length; }
            set
            {
                _length = value;
                CreateCylinder();
            }
        }

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                CreateCylinder();
            }
        }

        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                CreateCylinder();
            }
        }

        public Color SecondColor
        {
            get { return _secondColor; }
            set
            {
                _secondColor = value;
                CreateCylinder();
            }
        }

        protected virtual void CreateCylinder()
        {
            var verts = Math.Max(RadiusMultiplier, (int) (RadiusMultiplier*Radius));

            var offs = (float) (2*Math.PI)/(verts);

            Vertices = new VertexPositionColor[verts*3*2];
            Count = verts*3;

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

                Vertices[i*6 + 0] =
                    new VertexPositionColor(
                        Transform.PointToWorldSpace(Position, heading, up, side, new Vector3(0, x1, z1)), Color);
                Vertices[i*6 + 1] =
                    new VertexPositionColor(
                        Transform.PointToWorldSpace(Position, heading, up, side, new Vector3(Length, x1, z1)),
                        SecondColor);
                Vertices[i*6 + 2] =
                    new VertexPositionColor(
                        Transform.PointToWorldSpace(Position, heading, up, side, new Vector3(0, x1, z1)), Color);
                Vertices[i*6 + 3] =
                    new VertexPositionColor(
                        Transform.PointToWorldSpace(Position, heading, up, side, new Vector3(0, x2, z2)), Color);
                Vertices[i*6 + 4] =
                    new VertexPositionColor(
                        Transform.PointToWorldSpace(Position, heading, up, side, new Vector3(Length, x1, z1)),
                        SecondColor);
                Vertices[i*6 + 5] =
                    new VertexPositionColor(
                        Transform.PointToWorldSpace(Position, heading, up, side, new Vector3(Length, x2, z2)),
                        SecondColor);
            }
        }

        #region Implementation of IDrawablePart

        public bool IsVisible { get; set; }

        public void Draw(DrawingService drawingService, GameTime gameTime)
        {
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = _cameraService.View;
            _basicEffect.Projection = _cameraService.Projection;

            _basicEffect.CurrentTechnique.Passes[0].Apply();

            drawingService.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, Vertices, 0, Count);
        }

        #endregion
    }
}