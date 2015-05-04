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
    public class ConsoleService : DrawableGameComponent, IConsoleService
    {
        private const float AverageCharWidth = 25;

        private const int ConsoleMaxMessages = 200;
        private const float ConsoleDefaultLineHeight = 15.0f;
        private const float ConsoleScrollMargin = 5.0f;
        private const float ConsoleTextMargin = 15.0f;
        private const float ConsoleScrollWidth = 15.0f;
        private const float ConsoleFontScale = .6f;

        private const int ChatMaxMessages = 6;
        private const float ChatFontScale = .75f;
        private const float ChatShadowOffset = 2.5f;
        private const float ChatShadowColorScale = .3f;
        private const float ChatTextMargin = 4.0f;
        private const float ChatMaxWidth = .4f;
        private const float ChatVisibleTime = 8.5f;
        private const float ChatFadeInTime = 0.212f;
        private const float ChatFadeOutTime = 1.2f;

        private readonly char[] _terminators = {' ', '.', ',', ':', ';', '(', ')', '{', '}', '[', ']', '!', '?'};
        private readonly Texture2D _backgroundTexture;
        private readonly SpriteFont _font;
        private readonly Queue<ConsoleMessageLine> _consoleMessages = new Queue<ConsoleMessageLine>();
        private readonly Queue<ChatMessageLine> _chatMessages = new Queue<ChatMessageLine>();
        private readonly SpriteBatch _spriteBatch;
        private readonly ConsoleTraceListener _traceListener;
        private bool _isConsoleVisible;
        private KeyboardState _lastKeyboardState;
        private float _scrollableHeight;
        private Vector2 _scrollBarPosition;
        private Rectangle _scrollBarSize;
        private float _scrollPosition;
        private int _scrollWheel;

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

            _backgroundTexture.Dispose();
            _spriteBatch.Dispose();
        }

        #region WriteLine methods

        public void WriteLine(Color color, string message)
        {
            if (message == null) throw new ArgumentNullException("message");

            // Ensure line is actually only one line.
            var messageParts = message.Split('\r', '\n').Where(s => s.Length > 0).ToArray();
            if (messageParts.Length > 1)
            {
                foreach (var part in messageParts.Reverse())
                    WriteLine(color, part);

                return;
            }

            message = messageParts.Length != 0 ? messageParts[0] : string.Empty;

            // Remove illegal characters.
            message = message.Select(c => !_font.Characters.Contains(c))
                .Distinct()
                .Aggregate(message,
                    (current, c) =>
                        current.Replace(c.ToString(),
                            _font.DefaultCharacter.HasValue ? _font.DefaultCharacter.Value.ToString() : "?"));

            // Measure the string
            var maxWidth = Game.Window.ClientBounds.Width - ConsoleTextMargin*2 - ConsoleScrollMargin - ConsoleScrollWidth;
            Vector2 size = _font.MeasureString(message)*ConsoleFontScale;

            if (size.X > maxWidth)
            {
                var tmpLength = (int)(maxWidth / (AverageCharWidth * ConsoleFontScale));
                    // Asuming the average character is at maximum 10 pixels wide. This improved performance.

                while (tmpLength != -1 && tmpLength < message.Length)
                {
                    // Find first terminator.
                    var curLength = message.IndexOfAny(_terminators, tmpLength);
                    if (curLength != -1 && curLength >= message.Length) break;

                    // Include the terminator character.
                    curLength++;

                    // Measure string length so far.
                    var curMessage = message.Substring(0, curLength);
                    var curSize = _font.MeasureString(curMessage)*ConsoleFontScale;

                    // If string surpasses the maximum line width, break from the loop.
                    if (curSize.X > maxWidth) break;

                    tmpLength = curLength;
                }

                // Is split successful?
                if (tmpLength < message.Length)
                {
                    // Split string into two parts; Print both to the console.
                    WriteLine(color, message.Substring(0, tmpLength));
                    WriteLine(color, message.Substring(tmpLength));
                    return;
                }
            }

            _consoleMessages.Enqueue(new ConsoleMessageLine(color, message, size));

            if (_consoleMessages.Count > ConsoleMaxMessages)
                _consoleMessages.Dequeue();
        }

        private void WriteChatLine(Color color, string message)
        {
            if (message == null) throw new ArgumentNullException("message");

            // Ensure line is actually only one line.
            var messageParts = message.Split('\r', '\n').Where(s => s.Length > 0).ToArray();
            if (messageParts.Length > 1)
            {
                foreach (var part in messageParts.Reverse())
                    WriteChatLine(color, part);

                return;
            }

            message = messageParts.Length != 0 ? messageParts[0] : string.Empty;

            // Remove illegal characters.
            message = message.Select(c => !_font.Characters.Contains(c))
                .Distinct()
                .Aggregate(message,
                    (current, c) =>
                        current.Replace(c.ToString(),
                            _font.DefaultCharacter.HasValue ? _font.DefaultCharacter.Value.ToString() : "?"));

            // Measure the string
            var maxWidth = Game.Window.ClientBounds.Width * ChatMaxWidth - ChatTextMargin * 2;
            Vector2 size = _font.MeasureString(message) * ChatFontScale;

            if (size.X > maxWidth)
            {
                var tmpLength = (int)(maxWidth / (AverageCharWidth * ChatFontScale));
                // Asuming the average character is at maximum 10 pixels wide. This improved performance.

                while (tmpLength != -1 && tmpLength < message.Length)
                {
                    // Find first terminator.
                    var curLength = message.IndexOfAny(_terminators, tmpLength);
                    if (curLength != -1 && curLength >= message.Length) break;

                    // Include the terminator character.
                    curLength++;

                    // Measure string length so far.
                    var curMessage = message.Substring(0, curLength);
                    var curSize = _font.MeasureString(curMessage) * ChatFontScale;

                    // If string surpasses the maximum line width, break from the loop.
                    if (curSize.X > maxWidth) break;

                    tmpLength = curLength;
                }

                // Is split successful?
                if (tmpLength < message.Length)
                {
                    // Split string into two parts; Print both to the chat.
                    WriteChatLine(color, message.Substring(0, tmpLength));
                    WriteChatLine(color, message.Substring(tmpLength));
                    return;
                }
            }

            _chatMessages.Enqueue(new ChatMessageLine(color, message, size));

            if (_chatMessages.Count > ChatMaxMessages)
                _chatMessages.Dequeue();
        }
        
        public void WriteLine(Color color, Exception exception)
        {
            var tabs = string.Empty;
            while (exception != null)
            {
                WriteLine(color, string.Format("{0}[{2}] Exception: {1} @ {3}", tabs, exception.Message, exception.Source, exception.TargetSite));

                tabs += ">";
                exception = exception.InnerException;
            }
        }

        private static Color UInt32ToColor(UInt32 colorCode)
        {
            var a = (colorCode >> 8 * 3) & 0xFF;
            var r = (colorCode >> 8 * 2) & 0xFF;
            var g = (colorCode >> 8 * 1) & 0xFF;
            var b = (colorCode >> 8 * 0) & 0xFF;

            return new Color(new Vector4((float) r/byte.MaxValue, (float) g/byte.MaxValue, (float) b/byte.MaxValue,
                (float) a/byte.MaxValue));
        }

        [ScriptingFunction("logprintf")]
        public void LogPrintFormatted(AMXArgumentList arguments)
        {
            WriteLine(UInt32ToColor(arguments[0].AsUInt32()), DefaultFunctions.FormatString(arguments, 1));
        }

        [ScriptingFunction("chatprintf")]
        public void ChatPrintFormatted(AMXArgumentList arguments)
        {
            var color = UInt32ToColor(arguments[0].AsUInt32());
            var message = DefaultFunctions.FormatString(arguments, 1);
            WriteLine(color, message);
            WriteChatLine(color, message);
        }

        #endregion

        #region Overrides of GameComponent

        public override void Update(GameTime gameTime)
        {
            #region Console interaction

            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            if (!_lastKeyboardState.IsKeyDown(Keys.Tab) && keyboardState.IsKeyDown(Keys.Tab))
                _isConsoleVisible = !_isConsoleVisible;

            _lastKeyboardState = keyboardState;
            if (_isConsoleVisible)
            {

                var messagesTotalHeight = (ConsoleMaxMessages - _consoleMessages.Count)*ConsoleDefaultLineHeight +
                                          _consoleMessages.Sum(m => m.Size.Y);
                var screenHeight = Game.Window.ClientBounds.Height;
                _scrollableHeight = messagesTotalHeight - screenHeight;
                var pageSize = screenHeight/_scrollableHeight;

                var scrollPercentageOnScreen = 1 - (_scrollPosition/_scrollableHeight);

                var scrollBarHeight = (screenHeight - ConsoleScrollMargin*2)*pageSize;
                var scrollBarScrollableHeight = screenHeight - ConsoleScrollMargin*2 - scrollBarHeight;
                var scrollBarY = ConsoleScrollMargin + scrollBarScrollableHeight*scrollPercentageOnScreen;

                _scrollBarPosition = new Vector2(Game.Window.ClientBounds.Width - ConsoleScrollWidth - ConsoleScrollMargin, scrollBarY);
                _scrollBarSize = new Rectangle(0, 0, (int) ConsoleScrollWidth, (int) scrollBarHeight);

                if (keyboardState.IsKeyDown(Keys.PageUp))
                    _scrollPosition += (float) (gameTime.ElapsedGameTime.TotalSeconds*screenHeight)*5;

                if (keyboardState.IsKeyDown(Keys.PageDown))
                    _scrollPosition -= (float) (gameTime.ElapsedGameTime.TotalSeconds*screenHeight)*5;

                var deltascroll = mouseState.ScrollWheelValue - _scrollWheel;

                if (deltascroll != 0)
                    _scrollPosition += (float) (gameTime.ElapsedGameTime.TotalSeconds*screenHeight)*deltascroll/25;

                if (_scrollPosition < 0) _scrollPosition = 0;
                if (_scrollPosition > _scrollableHeight) _scrollPosition = _scrollableHeight;
            }

            _scrollWheel = mouseState.ScrollWheelValue;

            #endregion

            if (_chatMessages.Count > 0)
            {
                var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

                var isOutOfDate = _chatMessages.Aggregate(false,
                    (current, message) => current || (message.TimeAlive += deltaTime) > ChatVisibleTime);

                if (isOutOfDate)
                    _chatMessages.Dequeue();
            }
            base.Update(gameTime);
        }

        #endregion

        #region Overrides of DrawableGameComponent

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            var chatSizeSoFar = 0f;
            if (_chatMessages.Count > 0)
            {
                var newestMessage = _chatMessages.Last();
                var messageOffset = newestMessage.TimeAlive > ChatFadeInTime
                    ? 0
                    : newestMessage.Size.Y - (newestMessage.TimeAlive / ChatFadeInTime) * newestMessage.Size.Y;

                foreach (var msg in _chatMessages.Reverse())
                {
                    chatSizeSoFar += msg.Size.Y;

                    var y = Game.Window.ClientBounds.Height - ChatTextMargin - chatSizeSoFar + messageOffset;

                    var pos = new Vector2(ChatTextMargin, y);

                    var fadeInAlpha = Math.Min(msg.TimeAlive / ChatFadeInTime, 1);
                    var fadeOutAlpha = Math.Min((ChatVisibleTime - msg.TimeAlive) / ChatFadeOutTime, 1);
                    var alpha = Math.Min(fadeInAlpha, fadeOutAlpha);

                    var color = msg.Color*alpha;
//                    color.R = (byte)(color.R * alpha);
//                    color.G = (byte)(color.G * alpha);
//                    color.B = (byte)(color.B * alpha);
//
//                    color.A = (byte)(color.A * alpha);

                    var shadowColor = color;
                    shadowColor.R = (byte)(color.R * ChatShadowColorScale);
                    shadowColor.G = (byte)(color.G * ChatShadowColorScale);
                    shadowColor.B = (byte)(color.B * ChatShadowColorScale);

                    _spriteBatch.DrawString(_font, msg.Message, pos + new Vector2(ChatShadowOffset), shadowColor, 0, Vector2.Zero,
                        new Vector2(ChatFontScale),
                        SpriteEffects.None, 0);

                    _spriteBatch.DrawString(_font, msg.Message, pos, color, 0, Vector2.Zero,
                        new Vector2(ChatFontScale),
                        SpriteEffects.None, 0);
                }
            }

            if (_isConsoleVisible)
            {
                _spriteBatch.Draw(_backgroundTexture, Vector2.Zero,
                    new Rectangle(0, 0, Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height),
                    Color.Black*0.5f, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);

                _spriteBatch.Draw(_backgroundTexture, _scrollBarPosition, _scrollBarSize, Color.White);

                var consoleSizeSoFar = (ConsoleMaxMessages - _consoleMessages.Count)*ConsoleDefaultLineHeight + _scrollPosition;
                foreach (var msg in _consoleMessages)
                {
                    var y = -_scrollableHeight + consoleSizeSoFar;

                    _spriteBatch.DrawString(_font, msg.Message, new Vector2(ConsoleTextMargin, y), msg.Color, 0,
                        Vector2.Zero,
                        new Vector2(ConsoleFontScale), SpriteEffects.None, 0);

                    consoleSizeSoFar += msg.Size.Y;
                }

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
            ///     Initializes a new instance of the <see cref="T:System.Diagnostics.TraceListener" /> class.
            /// </summary>
            public ConsoleTraceListener(IConsoleService console)
            {
                _console = console;
            }

            /// <summary>
            ///     When overridden in a derived class, writes the specified message to the listener you create in the derived class.
            /// </summary>
            /// <param name="message">A message to write. </param>
            public override void Write(string message)
            {
                _console.WriteLine(Color.GreenYellow, message);
            }

            /// <summary>
            ///     When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a
            ///     line terminator.
            /// </summary>
            /// <param name="message">A message to write. </param>
            public override void WriteLine(string message)
            {
                _console.WriteLine(Color.GreenYellow, message);
            }

            #endregion
        }

        private struct ConsoleMessageLine
        {
            public ConsoleMessageLine(Color color, string message, Vector2 size) : this()
            {
                Color = color;
                Message = message;
                Size = size;
            }

            public Color Color { get; set; }
            public string Message { get; set; }
            public Vector2 Size { get; set; }
        }
        private class ChatMessageLine
        {
            public ChatMessageLine(Color color, string message, Vector2 size)
            {
                Color = color;
                Message = message;
                Size = size;
                TimeAlive = 0;
            }

            public Color Color { get; set; }
            public string Message { get; set; }
            public Vector2 Size { get; set; }
            public float TimeAlive { get; set; }
        }
    }
}