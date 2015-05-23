﻿// AIWorld
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
    public class DrawableText2D : IDrawable2D, IDrawableHasPosition, IDrawableHasText, IDrawableHasColor,
        IDrawableHasFont
    {
        private Vector2 _position;

        public DrawableText2D(Vector2 position, Color color, SpriteFont font, string text)
        {
            if (font == null) throw new ArgumentNullException("font");
            if (text == null) throw new ArgumentNullException("text");
            _position = position;
            Color = color;
            Font = font;
            Text = text;
        }

        #region Implementation of IDrawableHasColor

        public Color Color { get; set; }

        #endregion

        #region Implementation of IDrawableHasFont

        public SpriteFont Font { get; set; }

        #endregion

        #region Implementation of IDrawableHasPosition

        public Vector3 Position
        {
            get { return new Vector3(_position, 0); }
            set { _position = new Vector2(value.X, value.Y); }
        }

        #endregion

        #region Implementation of IDrawableHasText

        public string Text { get; set; }

        #endregion

        #region Implementation of IDrawablePart

        public bool IsVisible { get; set; }

        public void Draw(DrawingService drawingService, SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.DrawString(Font, Text, _position, Color);
        }

        #endregion
    }
    public class DrawableText3D : IDrawable2D, IDrawableHasPosition, IDrawableHasText, IDrawableHasColor,
        IDrawableHasFont
    {
        private readonly ICameraService _cameraService;

        public DrawableText3D(ICameraService cameraService, Vector3 position, Color color, SpriteFont font, string text)
        {
            _cameraService = cameraService;
            if (font == null) throw new ArgumentNullException("font");
            if (text == null) throw new ArgumentNullException("text");
            Position = position;
            Color = color;
            Font = font;
            Text = text;
        }

        #region Implementation of IDrawableHasColor

        public Color Color { get; set; }

        #endregion

        #region Implementation of IDrawableHasFont

        public SpriteFont Font { get; set; }

        #endregion

        #region Implementation of IDrawableHasPosition

        public Vector3 Position { get; set; }

        #endregion

        #region Implementation of IDrawableHasText

        public string Text { get; set; }

        #endregion

        #region Implementation of IDrawablePart

        public bool IsVisible { get; set; }

        public void Draw(DrawingService drawingService, SpriteBatch spriteBatch, GameTime gameTime)
        {
            var drp = drawingService.GraphicsDevice.Viewport.Project(Position, _cameraService.Projection, _cameraService.View,
                Matrix.Identity);
            spriteBatch.DrawString(Font, Text, new Vector2(drp.X, drp.Y) - Font.MeasureString(Text) * new Vector2(0.5f, 1), Color);
        }

        #endregion
    }
}