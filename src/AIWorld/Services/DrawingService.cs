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
using System.Linq;
using AIWorld.Core;
using AIWorld.Drawable;
using AIWorld.Scripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Services
{
    public class DrawingService : DrawableGameComponent, IDrawingService
    {
        private readonly ICameraService _cameraService;
        private readonly Pool<IDrawablePart> _drawables = new Pool<IDrawablePart>();
        private readonly SortedList<int, IDrawablePart> _drawDrawables = new SortedList<int, IDrawablePart>();
        private readonly Dictionary<string, SpriteFont> _fonts = new Dictionary<string, SpriteFont>();
        private readonly SpriteBatch _spriteBatch;
        private int _currentDrawOrder;

        public DrawingService(Game game, ICameraService cameraService) : base(game)
        {
            _cameraService = cameraService;
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            DrawOrder = 10;
        }

        private bool IsValidDrawableId(int drawableid)
        {
            return _drawables[drawableid] != null;
        }

        private static Color UInt32ToColor(UInt32 colorCode)
        {
            var a = (colorCode >> 8*0) & 0xFF;
            var r = (colorCode >> 8*3) & 0xFF;
            var g = (colorCode >> 8*2) & 0xFF;
            var b = (colorCode >> 8*1) & 0xFF;

            var color = Color.White;
            color.A = (byte) (colorCode);
            color.R = (byte) (colorCode >> 24);
            color.G = (byte) (colorCode >> 16);
            color.B = (byte) (colorCode >> 8);
            return color;
        }

        private SpriteFont GetFont(string name)
        {
            return _fonts.ContainsKey(name) ? _fonts[name] : (_fonts[name] = Game.Content.Load<SpriteFont>(name));
        }

        #region Overrides of DrawableGameComponent

        public override void Draw(GameTime gameTime)
        {
            foreach (var part in _drawDrawables.Where(p => p.Value is IDrawable3D).Select(p => p.Value as IDrawable3D))
            {
                part.Draw(this, gameTime);
            }

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            foreach (var part in _drawDrawables.Where(p => p.Value is IDrawable2D).Select(p => p.Value as IDrawable2D))
            {
                part.Draw(this, _spriteBatch, gameTime);
            }
            _spriteBatch.End();
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;


            base.Draw(gameTime);
        }

        #endregion

        #region Creating

        [ScriptingFunction]
        public int CreateDrawableText2D(float x, float y, uint color, string font, string text)
        {
            return _drawables.Add(new DrawableText2D(new Vector2(x, y), UInt32ToColor(color), GetFont(font), text));
        }

        [ScriptingFunction]
        public int CreateDrawableText3D(float x, float y, float z, uint color, string font, string text)
        {
            return
                _drawables.Add(new DrawableText3D(_cameraService, new Vector3(x, y, z), UInt32ToColor(color),
                    GetFont(font), text));
        }

        [ScriptingFunction]
        public int CreateDrawableLine(float x1, float y1, float z1, float x2, float y2, float z2, uint color1,
            uint color2)
        {
            return
                _drawables.Add(new DrawableLine(_cameraService, GraphicsDevice, new Vector3(x1, y1, z1),
                    new Vector3(x2, y2, z2), UInt32ToColor(color1), UInt32ToColor(color2)));
        }

        [ScriptingFunction]
        public int CreateDrawableLineCylinder(float x, float y, float z, float hx, float hy, float hz, float length,
            float radius, uint color1, uint color2)
        {
            return
                _drawables.Add(new DrawableLineCylinder(_cameraService, GraphicsDevice, new Vector3(x, y, z),
                    new Vector3(hx, hy, hz), length, radius, UInt32ToColor(color1),
                    UInt32ToColor(color2)));
        }

        [ScriptingFunction]
        public int CreateDrawableLineCone(float x, float y, float z, float hx, float hy, float hz, float length,
            float radius, uint color1, uint color2)
        {
            return
                _drawables.Add(new DrawableLineCone(_cameraService, GraphicsDevice, new Vector3(x, y, z),
                    new Vector3(hx, hy, hz), length, radius, UInt32ToColor(color1),
                    UInt32ToColor(color2)));
        }

        [ScriptingFunction]
        public int CreateDrawableLineSphere(float x, float y, float z, float radius, uint color1, uint color2)
        {
            return
                _drawables.Add(new DrawableLineSphere(_cameraService, GraphicsDevice, new Vector3(x, y, z), radius,
                    UInt32ToColor(color1), UInt32ToColor(color2)));
        }

        #endregion

        #region Updating

        private bool SetValue<T>(int drawableid, Action<T> action) where T : IDrawablePart
        {
            if (!IsValidDrawableId(drawableid)) return false;

            var value = _drawables[drawableid];
            if (!(value is T)) return false;

            action((T) value);
            return true;
        }

        [ScriptingFunction]
        public bool SetDrawablePosition(int drawableid, float x, float y, float z)
        {
            return SetValue<IDrawableHasPosition>(drawableid, p => p.Position = new Vector3(x, y, z));
        }

        [ScriptingFunction]
        public bool SetDrawablePosition2(int drawableid, float x, float y, float z)
        {
            return SetValue<IDrawableHasSecondPosition>(drawableid, p => p.SecondPosition = new Vector3(x, y, z));
        }

        [ScriptingFunction]
        public bool SetDrawableScale(int drawableid, float x, float y)
        {
            return SetValue<IDrawableHasScale>(drawableid, p => p.Scale = new Vector2(x, y));
        }

        [ScriptingFunction]
        public bool SetDrawableText(int drawableid, string value)
        {
            return SetValue<IDrawableHasText>(drawableid, p => p.Text = value);
        }

        [ScriptingFunction]
        public bool SetDrawableColor(int drawableid, uint value)
        {
            return SetValue<IDrawableHasColor>(drawableid, p => p.Color = UInt32ToColor(value));
        }

        [ScriptingFunction]
        public bool SetDrawableColor2(int drawableid, uint value)
        {
            return SetValue<IDrawableHasSecondColor>(drawableid, p => p.SecondColor = UInt32ToColor(value));
        }

        [ScriptingFunction]
        public bool SetDrawableFont(int drawableid, string value)
        {
            return SetValue<IDrawableHasFont>(drawableid, p => p.Font = GetFont(value));
        }

        [ScriptingFunction]
        public bool SetDrawableRadius(int drawableid, float value)
        {
            return SetValue<IDrawableHasRadius>(drawableid, p => p.Radius = value);
        }

        [ScriptingFunction]
        public bool SetDrawableLength(int drawableid, float value)
        {
            return SetValue<IDrawableHasLength>(drawableid, p => p.Length = value);
        }

        #endregion

        #region Removing, showing hiding

        [ScriptingFunction]
        public bool HideDrawable(int drawableid)
        {
            if (!IsValidDrawableId(drawableid)) return false;

            if (!_drawables[drawableid].IsVisible)
                return true;
            var rmidx = _drawDrawables.IndexOfValue(_drawables[drawableid]);
            _drawDrawables.RemoveAt(rmidx);
            _drawables[drawableid].IsVisible = false;

            return true;
        }

        [ScriptingFunction]
        public bool ShowDrawable(int drawableid)
        {
            if (!IsValidDrawableId(drawableid)) return false;

            if (_drawables[drawableid].IsVisible)
                return true;

            _drawables[drawableid].IsVisible = true;
            _drawDrawables.Add(_currentDrawOrder++, _drawables[drawableid]);
            return true;
        }

        [ScriptingFunction]
        public bool DestroyDrawable(int drawableid)
        {
            if (!IsValidDrawableId(drawableid)) return false;

            if (_drawables[drawableid].IsVisible)
                _drawDrawables.RemoveAt(_drawDrawables.IndexOfValue(_drawables[drawableid]));

            return _drawables.Remove(drawableid);
        }

        #endregion
    }
}