using System;
using System.Diagnostics;
using AIWorld.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Drawable
{
    public class DrawableText3D : IDrawable2D, IDrawableHasPosition, IDrawableHasText, IDrawableHasColor,
        IDrawableHasFont, IDrawableHasScale
    {
        private readonly ICameraService _cameraService;

        public DrawableText3D(ICameraService cameraService, Vector3 position, Color color, SpriteFont font, string text)
        {
            _cameraService = cameraService;
            if (font == null) throw new ArgumentNullException("font");
            if (text == null) throw new ArgumentNullException("text");
            Position = position;
            Color = color;
            Scale=Vector2.One;
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
            spriteBatch.DrawString(Font, Text, new Vector2(drp.X, drp.Y) - Font.MeasureString(Text)*Scale*new Vector2(0.5f, 1),
                Color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }

        #endregion

        #region Implementation of IDrawableHasScale

        public Vector2 Scale { get; set; }

        #endregion
    }
}