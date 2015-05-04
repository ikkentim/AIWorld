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
using AMXWrapper;
using Microsoft.Xna.Framework;

namespace AIWorld.Scripting
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class DefaultFunctions : IScriptingNatives
    {
        public readonly Random Random = new Random();

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

            // Format eg: 'xpos:f,ypos,f;count;d'

            var formatSeperated = format.Split(',', '.', ' ', ';', ':', '-');
            var formatTypes = "difcb".ToCharArray();

            for (var j = 0; j < formatSeperated.Length; j++)
            {
                var name = formatSeperated[j];
                var type = formatSeperated.Length > j + 1
                    ? (
                        formatSeperated[j + 1].Trim().Length == 1
                            ? (
                                formatTypes.Contains(formatSeperated[j + 1].Trim().First())
                                    ? formatSeperated[++j].Trim().First()
                                    : 'd' // Default if unknown type.
                                )
                            : 'd' // Default if type has no length.
                        )
                    : 'd'; // Default if last in format.

                switch (type)
                {
                    case 'd':
                    case 'i':
                    case 'c':
                    case 'b':
                        var iValue = arguments[i++].AsCellPtr().Get().AsInt32();

                        CellPtr iCell;
                        if (scripted.Script.PublicVars.TryGetValue(name, out iCell))
                            iCell.Set(iValue);
                        break;
                    case 'f':
                        var fValue = arguments[i++].AsCellPtr().Get().AsFloat();
                        
                        CellPtr fCell;
                        if (scripted.Script.PublicVars.TryGetValue(name, out fCell))
                            fCell.Set(Cell.FromFloat(fValue));
                        break;
                }
            }
        }

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

        [ScriptingFunction("frand")]
        public float FloatRandom()
        {
            return (float) Random.NextDouble();
        }

        [ScriptingFunction("distance")]
        public float Distance(float x1, float y1, float x2, float y2)
        {
            return (new Vector2(x1, y1) - new Vector2(x2, y2)).Length();
        }

        [ScriptingFunction("distancesquared")]
        public float DistanceSquared(float x1, float y1, float x2, float y2)
        {
            return (new Vector2(x1, y1) - new Vector2(x2, y2)).LengthSquared();
        }
    }
}