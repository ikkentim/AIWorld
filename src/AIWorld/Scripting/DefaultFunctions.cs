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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using AIWorld.Services;
using AMXWrapper;
using Microsoft.Xna.Framework;

namespace AIWorld.Scripting
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class DefaultFunctions : IScriptingNatives
    {
        private readonly Random _random = new Random();

        private static string StringPrintFormatted(string input, object[] inpVars)
        {
            var i = 0;
            input = Regex.Replace(input.Replace("{", "{{").Replace("}", "}}"), "%.", m => ("{" + i++ + "}"));
            return string.Format(input, inpVars);
        }

        private static IEnumerable<char> FormatChars(string input)
        {
            for (var i = 0; i < input.Length - 1; i++)
            {
                if (input[i] == '%')
                    yield return input[++i];
            }
        }

        public static void SetVariables(IScripted scripted, AMXArgumentList arguments, int formatIndex = 0)
        {
            if (scripted == null) throw new ArgumentNullException("scripted");
            var format = arguments[formatIndex].AsString();
            var i = formatIndex + 1;

            // Format eg: 'xpos,ypos;count'
            var formatSeperated = format.Split(',', '.', ' ', ';', ':', '-');
            for (var j = 0; j < Math.Min(formatSeperated.Length, arguments.Length - formatIndex - 1); j++)
            {
                var name = formatSeperated[j];
                var value = arguments[i++].AsCellPtr().Get();

                CellPtr cellPtr;
                if (scripted.Script.PublicVars.TryGetValue(name, out cellPtr))
                    cellPtr.Set(value);
            }
        }

        public static int? CallFunctionOnScript(ScriptBox script, IConsoleService consoleService,
            AMXArgumentList arguments)
        { // name,format,...
            if (arguments.Length < 2)
                return null;

            var function = arguments[0].AsString();
            var format = arguments[1].AsString();
            var publicFunction = script.Publics.ContainsKey(function) ? script.Publics[function] : null;

            if (publicFunction == null) return null;

            var strings = new List<CellPtr>();
            var i = 2;

            var pars = new List<object>();
            foreach (var t in format.TakeWhile(t => i < arguments.Length))
            {
                switch (t)
                {
                    case 'd':
                    case 'i':
                    case 'f':
                        pars.Add(arguments[i++].AsCellPtr().Get());
                        break;
                    case 's':
                        pars.Add(arguments[i++].AsString());
                        break;
                }
            }

            pars.Reverse();
            foreach (var p in pars)
            {
                if (p is string)
                    strings.Add(script.Push(p as string));
                else
                    script.Push((Cell) p);
            }

            int? result = null;
            try
            {
                result = publicFunction.Execute();
            }
            catch (Exception e)
            {
                consoleService.WriteLine(Color.Red, e);
            }

            foreach (var str in strings)
                script.Release(str);

            return result;
        }

        /// <summary>
        /// Formats a string.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="formatIndex">Index of the format.</param>
        /// <returns>Formatted string.</returns>
        public static string FormatString(AMXArgumentList arguments, int formatIndex = 0)
        {
            var format = arguments[formatIndex].AsString();
            var i = formatIndex + 1;
            var parms = FormatChars(format).Select(c =>
            {
                switch (c)
                {
                    case 'd':
                    case 'i':
                        return (object) arguments[i++].AsCellPtr().Get().AsInt32();
                    case 's':
                        return (object) arguments[i++].AsString();
                    case 'f':
                        return (object) arguments[i++].AsCellPtr().Get().AsFloat();
                    case 'c':
                        return (char) arguments[i++].AsCellPtr().Get().AsInt32();
                    case 'x':
                        return string.Format("0x{0:X}", arguments[i++].AsCellPtr().Get().AsInt32());
                    case 'b':
                        return "0b" + Convert.ToString(arguments[i++].AsCellPtr().Get().AsInt32(), 2);
                    case '%':
                        return '%';
                    default:
                        i++;
                        return (object) 0;
                }
            }).ToArray();

            return StringPrintFormatted(format, parms);
        }

        /// <summary>
        /// Gets a random floating point value.
        /// </summary>
        /// <returns>Random floating point value.</returns>
        [ScriptingFunction("frand")]
        public float FloatRandom()
        {
            return (float) _random.NextDouble();
        }

        [ScriptingFunction("floatatan2")]
        public float FloatAtan2(float y, float x)
        {
            return (float) Math.Atan2(y, x);
        }

        [ScriptingFunction("floatsin2")]
        public float FloatSin2(float a)
        {
            return (float)Math.Sin(a);
        }
        [ScriptingFunction("floatcos2")]
        public float FloatCos2(float a)
        {
            return (float)Math.Cos(a);
        }
        /// <summary>
        /// Gets the distance between the specified points..
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <returns>The distance</returns>
        [ScriptingFunction("distance")]
        public float Distance(float x1, float y1, float x2, float y2)
        {
            return (new Vector2(x1, y1) - new Vector2(x2, y2)).Length();
        }

        /// <summary>
        /// Gets the squared distance between the specified points.
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <returns>The squared distance.</returns>
        [ScriptingFunction("distancesquared")]
        public float DistanceSquared(float x1, float y1, float x2, float y2)
        {
            return (new Vector2(x1, y1) - new Vector2(x2, y2)).LengthSquared();
        }
    }
}