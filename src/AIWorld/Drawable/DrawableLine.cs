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

using AIWorld.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Drawable
{
    public class DrawableLine : IDrawable3D, IDrawableHasPosition, IDrawableHasSecondPosition, IDrawableHasColor,
        IDrawableHasSecondColor
    {
        private readonly BasicEffect _basicEffect;
        private readonly ICameraService _cameraService;
        private readonly VertexPositionColor[] _vertices;

        public DrawableLine(ICameraService cameraService, GraphicsDevice graphicsDevice, Vector3 position,
            Vector3 secondPosition, Color color, Color secondColor)
        {
            _cameraService = cameraService;
            _vertices = new[]
            {new VertexPositionColor(position, color), new VertexPositionColor(secondPosition, secondColor)};


            _basicEffect = new BasicEffect(graphicsDevice);
        }

        public Color Color
        {
            get { return _vertices[0].Color; }
            set { _vertices[0].Color = value; }
        }

        public Vector3 Position
        {
            get { return _vertices[0].Position; }
            set { _vertices[0].Position = value; }
        }

        public Color SecondColor
        {
            get { return _vertices[1].Color; }
            set { _vertices[1].Color = value; }
        }

        public Vector3 SecondPosition
        {
            get { return _vertices[1].Position; }
            set { _vertices[1].Position = value; }
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

            drawingService.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _vertices, 0, 1);
        }

        #endregion
    }
}