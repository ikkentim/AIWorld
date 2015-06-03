using System;
using System.Diagnostics;
using AIWorld.Helpers;
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
            if (cameraService == null) throw new ArgumentNullException("cameraService");
            if (font == null) throw new ArgumentNullException("font");
            if (text == null) throw new ArgumentNullException("text");

            _cameraService = cameraService;
            Position = position;
            Color = color;
            Scale = Vector2.One;
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
            var drawPosition = drawingService.GraphicsDevice.Viewport.Project(Position, _cameraService.Projection,
                _cameraService.View, Matrix.Identity);

            // Ensure the drawing position is within the viewport and in front of the camera.
            if (!(drawPosition.X >= 0) || !(drawPosition.Y >= 0) ||
                !(drawPosition.X <= drawingService.Game.GraphicsDevice.Viewport.Width) ||
                !(drawPosition.Y <= drawingService.Game.GraphicsDevice.Viewport.Height) || drawPosition.Z >= 1)
                return;

            // Measure the string for aligning the drawing position.
            var measuredSize = Font.MeasureString(Text);

            // drawPosition is the bottom center point of the label. Move the position accordingly based on the
            // measured size and the scale of the text label.
            spriteBatch.DrawString(Font, Text,
                drawPosition.ToVector2XY() - measuredSize*Scale*new Vector2(0.5f, 1),
                Color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }

        #endregion

        #region Implementation of IDrawableHasScale

        public Vector2 Scale { get; set; }

        #endregion
    }
}