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
using AIWorld.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Drawable
{
    public class DrawableLineSphere : IDrawable3D, IDrawableHasPosition, IDrawableHasColor, IDrawableHasSecondColor
    {
        protected const int RadiusMultiplier = 16;
        private readonly BasicEffect _basicEffect;
        private readonly ICameraService _cameraService;
        private Color _color;
        private Vector3 _direction;
        private Vector3 _position;
        private float _radius;
        private Color _secondColor;

        public DrawableLineSphere(ICameraService cameraService, GraphicsDevice graphicsDevice, Vector3 position,
            float radius, Color color, Color secondColor)
        {
            _position = position;
            _radius = radius;
            _color = color;
            _secondColor = secondColor;

            _cameraService = cameraService;

            CreateCylinder();

            _basicEffect = new BasicEffect(graphicsDevice);
        }

        protected VertexPositionColor[] Vertices { get; set; }
        protected int Count { get; set; }

        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                CreateCylinder();
            }
        }

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

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
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

            var offs = (float) (2*Math.PI)/verts;

            Vertices = new VertexPositionColor[verts*(verts/2)*4];
            Count = verts*(verts/2)*2;

            for (var r = 0; r < verts/2; r++)
            {
                var matrix = Matrix.CreateRotationY(r*offs);
                var height = (float) Math.Cos(r*offs)*Radius;
                var multiplier = (float) Math.Sin(r*offs);
                var color = Color.Lerp(SecondColor, Color, (float) Math.Cos(r*offs));

                for (var i = 0; i < verts; i++)
                {
                    var x1 = (float) Math.Cos(i*offs)*Radius;
                    var y1 = (float) Math.Sin(i*offs)*Radius;

                    var x2 = (float) Math.Cos(i*offs + offs)*Radius;
                    var y2 = (float) Math.Sin(i*offs + offs)*Radius;

                    var v1 = new Vector3(y1, x1, 0);
                    var v2 = new Vector3(y2, x2, 0);

                    var c1 = Color.Lerp(SecondColor, Color, (float) Math.Abs(i - (verts/2))/(verts/2));
                    var c2 = Color.Lerp(SecondColor, Color, (float) Math.Abs((i + 1) - (verts/2))/(verts/2));

                    var startidx = r*verts*4 + i*4;

                    // Verticals
                    Vertices[startidx + 0] = new VertexPositionColor(Position + Vector3.Transform(v1, matrix),
                        c1);
                    Vertices[startidx + 1] = new VertexPositionColor(Position + Vector3.Transform(v2, matrix),
                        c2);

                    // Horizontals
                    Vertices[startidx + 2] = new VertexPositionColor(
                        Position + new Vector3(x1*multiplier, height, y1*multiplier), color);
                    Vertices[startidx + 3] = new VertexPositionColor(
                        Position + new Vector3(x2*multiplier, height, y2*multiplier), color);
                }
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