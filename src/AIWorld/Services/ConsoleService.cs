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
using System.Diagnostics;
using System.Linq;
using AIWorld.Scripting;
using AMXWrapper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AIWorld.Services
{
    public class ConsoleService : DrawableGameComponent, IConsoleService, IScriptingNatives
    {
        private const int MaxMessages = 200;
        private const float DefaultLineHeight = 15.0f;
        private const float ScrollMargin = 5.0f;
        private const float ScrollWidth = 15.0f;
        private const float FontScale = .6f;
        private readonly Texture2D _backgroundTexture;
        private readonly SpriteFont _font;
        private readonly Queue<ConsoleMessage> _messages = new Queue<ConsoleMessage>();
        private readonly SpriteBatch _spriteBatch;
        private bool _isConsoleVisible;
        private KeyboardState _lastKeyboardState;
        private float _scrollableHeight;
        private Vector2 _scrollBarPosition;
        private Rectangle _scrollBarSize;
        private float _scrollPosition;
        private int _scrollWheel;
        private readonly ConsoleTraceListener _traceListener;

        public ConsoleService(Game game) : base(game)
        {
            DrawOrder = int.MaxValue;

            _backgroundTexture = new Texture2D(GraphicsDevice, 1, 1);
            _backgroundTexture.SetData(new[] {Color.Black});
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Game.Content.Load<SpriteFont>("fonts/consolas");

            Debug.Listeners.Add(_traceListener = new ConsoleTraceListener(this));
        }

        ~ConsoleService()
        {
            Debug.Listeners.Remove(_traceListener);
        }

        public void WriteLine(Color color, string message)
        {
            if (message == null) throw new ArgumentNullException("message");

            var item = new ConsoleMessage(color, message)
            {
                Size = _font.MeasureString(message)*FontScale
            };

            _messages.Enqueue(item);

            if (_messages.Count > MaxMessages)
                _messages.Dequeue();
        }

        [ScriptingFunction("logprintf")]
        public void LogPrintF(AMXArgumentList arguments)
        {
            var colorCode = arguments[0].AsUInt32();

            var a = (colorCode >> 8*3) & 0xFF;
            var r = (colorCode >> 8*2) & 0xFF;
            var g = (colorCode >> 8*1) & 0xFF;
            var b = (colorCode >> 8*0) & 0xFF;

            var color =
                new Color(new Vector4((float) r/byte.MaxValue, (float) g/byte.MaxValue, (float) b/byte.MaxValue,
                    (float) a/byte.MaxValue));

            var message = DefaultFunctions.FormatString(arguments, 1);

            WriteLine(color, message);
        }

        #region Overrides of GameComponent

        public override void Update(GameTime gameTime)
        {
            var ms = Mouse.GetState();


            var kb = Keyboard.GetState();

            if (!_lastKeyboardState.IsKeyDown(Keys.Tab) && kb.IsKeyDown(Keys.Tab))
                _isConsoleVisible = !_isConsoleVisible;

            _lastKeyboardState = kb;
            if (_isConsoleVisible)
            {
                var messagesTotalHeight = (MaxMessages - _messages.Count)*DefaultLineHeight +
                                          _messages.Sum(m => m.Size.Y);
                var screenHeight = Game.Window.ClientBounds.Height;
                _scrollableHeight = messagesTotalHeight - screenHeight;
                var pageSize = screenHeight/_scrollableHeight;

                var scrollPercentageOnScreen = 1 - (_scrollPosition/_scrollableHeight);

                var scrollBarHeight = (screenHeight - ScrollMargin*2)*pageSize;
                var scrollBarScrollableHeight = screenHeight - ScrollMargin*2 - scrollBarHeight;
                var scrollBarY = ScrollMargin + scrollBarScrollableHeight*scrollPercentageOnScreen;

                _scrollBarPosition = new Vector2(Game.Window.ClientBounds.Width - ScrollWidth - ScrollMargin, scrollBarY);
                _scrollBarSize = new Rectangle(0, 0, (int) ScrollWidth, (int) scrollBarHeight);

                if (kb.IsKeyDown(Keys.PageUp))
                    _scrollPosition += (float) (gameTime.ElapsedGameTime.TotalSeconds*screenHeight)*5;

                if (kb.IsKeyDown(Keys.PageDown))
                    _scrollPosition -= (float) (gameTime.ElapsedGameTime.TotalSeconds*screenHeight)*5;

                var deltascroll = ms.ScrollWheelValue - _scrollWheel;

                if (deltascroll != 0)
                    _scrollPosition += (float) (gameTime.ElapsedGameTime.TotalSeconds*screenHeight)*deltascroll/25;

                if (_scrollPosition < 0) _scrollPosition = 0;
                if (_scrollPosition > _scrollableHeight) _scrollPosition = _scrollableHeight;
            }

            _scrollWheel = ms.ScrollWheelValue;
            base.Update(gameTime);
        }

        #endregion

        #region Overrides of DrawableGameComponent

        public override void Draw(GameTime gameTime)
        {
            if (!_isConsoleVisible)
            {
                base.Draw(gameTime);
                return;
            }

            _spriteBatch.Begin();
            _spriteBatch.Draw(_backgroundTexture, Vector2.Zero,
                new Rectangle(0, 0, Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height), Color.Black*0.5f);

            _spriteBatch.Draw(_backgroundTexture, _scrollBarPosition, _scrollBarSize, Color.White);

            var sizeSoFar = (MaxMessages - _messages.Count)*DefaultLineHeight + _scrollPosition;
            foreach (var msg in _messages)
            {
                var y = -_scrollableHeight + sizeSoFar;

                _spriteBatch.DrawString(_font, msg.Message, new Vector2(0, y), msg.Color, 0, Vector2.Zero,
                    new Vector2(FontScale), SpriteEffects.None, 0);

                sizeSoFar += msg.Size.Y;
            }

            _spriteBatch.End();
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }

        #endregion

        private class ConsoleTraceListener : TraceListener
        {
            private readonly IConsoleService _console;

            #region Overrides of TraceListener

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Diagnostics.TraceListener"/> class.
            /// </summary>
            public ConsoleTraceListener(IConsoleService console)
            {
                _console = console;
            }

            /// <summary>
            /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
            /// </summary>
            /// <param name="message">A message to write. </param>
            public override void Write(string message)
            {
                _console.WriteLine(Color.GreenYellow, message);
            }

            /// <summary>
            /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
            /// </summary>
            /// <param name="message">A message to write. </param>
            public override void WriteLine(string message)
            {
                _console.WriteLine(Color.GreenYellow, message);
            }

            #endregion
        }

        private struct ConsoleMessage
        {
            public ConsoleMessage(Color color, string message) : this()
            {
                Color = color;
                Message = message;
            }

            public Color Color { get; set; }
            public string Message { get; set; }
            public Vector2 Size { get; set; }
        }
    }
}