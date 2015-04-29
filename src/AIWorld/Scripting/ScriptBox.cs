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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AMXWrapper;

namespace AIWorld.Scripting
{
    public class ScriptBox : AMX, IEnumerable<KeyValuePair<string, Cell>>
    {
        private static readonly DefaultFunctions _defaultFunctions = new DefaultFunctions();

        private readonly Dictionary<string, CellPtr> _publicVars = new Dictionary<string, CellPtr>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptBox" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        public ScriptBox(string type, string name) : base(string.Format("{0}/{1}.amx", type, name))
        {
            LoadLibrary(AMXDefaultLibrary.Core | AMXDefaultLibrary.Float | AMXDefaultLibrary.String |
                        AMXDefaultLibrary.Time);

            Register(_defaultFunctions);

            // Prepare public vars table
            for (var i = 0; i < PublicVarCount; i++)
            {
                string varname;
                var ptr = GetPublicVar(i, out varname);
                _publicVars[varname] = ptr;
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="Cell" /> with the specified name.
        /// </summary>
        public Cell this[string name]
        {
            get { return _publicVars[name].Get(); }
            set { _publicVars[name].Set(value); }
        }

        public void Register(AMXNativeFunction function)
        {
            if (function == null) throw new ArgumentNullException("function");
            if (function.Method.Name.Any(n => n == '<' || n == '>'))
                throw new ArgumentException("Invalid method name");

            Register(function.Method.Name, function);
        }

        private Func<Cell,object> CreateInTypeCaster(ParameterInfo info)
        {
            if (!info.ParameterType.IsByRef)
            {
                if (info.ParameterType == typeof (string)) return cell => cell.AsString();
                if (info.ParameterType == typeof (int)) return cell => cell.AsInt32();
                if (info.ParameterType == typeof (float)) return cell => cell.AsFloat();
                if (info.ParameterType == typeof (IntPtr)) return cell => cell.AsIntPtr();
                if (info.ParameterType == typeof (CellPtr)) return cell => cell.AsCellPtr();

                throw new ArgumentException("Invalid argument type " + info.ParameterType);
            }

            return cell => null;
        }

        private Action<Cell, object> CreateOutTypeCaster(ParameterInfo info)
        {
            if (info.ParameterType.IsByRef)
            {
                if (info.ParameterType.GetElementType() == typeof (int)) return (cell, value) => { cell.AsCellPtr().Set((int) value); };
                if (info.ParameterType.GetElementType() == typeof(float))
                    return (cell, value) => { cell.AsCellPtr().Set(Cell.FromFloat((float) value)); };
                throw new ArgumentException("Invalid argument type " + info.ParameterType);
            }
            return null;
        }

        public void Register(IScriptingNatives instance)
        {
            if (instance == null) throw new ArgumentNullException("instance");

            var properties =
                instance.GetType()
                    .GetProperties()
                    .Where(m => m.GetCustomAttributes(typeof(ScriptingFunctionAttribute), true).Any());

            foreach (var property in properties)
            {
                var attribute =
                    property.GetCustomAttributes(typeof (ScriptingFunctionAttribute), true).First() as
                        ScriptingFunctionAttribute;

                // Indexers are illegal
                if (property.GetIndexParameters().Length > 0) continue;

                var name = attribute.Name ?? property.Name;

                var propertyLocalCopy = property;

                if (property.PropertyType == typeof (string))
                {

                    Register(string.Format("Get{0}", name), (amx, arguments) =>
                    {
                        if (arguments.Length < 2)
                            return -1;

                        var length = arguments[1].AsInt32() - 1;
                        if (length <= 0) return -1;

                        var value = propertyLocalCopy.GetValue(instance, null) as string;
                        if (value == null) return -1;

                        SetString(arguments[0].AsCellPtr(), value.Length > length ? value.Substring(0, length) : value,
                            false);

                        return value.Length;
                    });

                    if (property.GetSetMethod() != null)
                        Register(string.Format("Set{0}", name), (amx, arguments) =>
                        {
                            if (arguments.Length < 1)
                                return 0;

                            propertyLocalCopy.SetValue(instance, arguments[0].AsString(), null);

                            return 1;
                        });
                }
                else if (property.PropertyType == typeof (int))
                {
                    Register(string.Format("Get{0}", name),
                        (amx, arguments) => (int) propertyLocalCopy.GetValue(instance, null));

                    if (property.GetSetMethod() != null)
                        Register(string.Format("Set{0}", name), (amx, arguments) =>
                        {
                            if (arguments.Length < 1)
                                return 0;

                            propertyLocalCopy.SetValue(instance, arguments[0].AsInt32(), null);

                            return 1;
                        });
                }
                else if (property.PropertyType == typeof (bool))
                {
                    Register(string.Format("Get{0}", name),
                        (amx, arguments) => (bool) propertyLocalCopy.GetValue(instance, null) ? 1 : 0);

                    if (property.GetSetMethod() != null)
                        Register(string.Format("Set{0}", name), (amx, arguments) =>
                        {
                            if (arguments.Length < 1)
                                return 0;

                            propertyLocalCopy.SetValue(instance, arguments[0].AsInt32() != 0, null);

                            return 1;
                        });
                }
                else if (property.PropertyType == typeof (float))
                {
                    Register(string.Format("Get{0}", name),
                        (amx, arguments) => Cell.FromFloat((float) propertyLocalCopy.GetValue(instance, null)).AsInt32());

                    if (property.GetSetMethod() != null)
                        Register(string.Format("Set{0}", name), (amx, arguments) =>
                        {
                            if (arguments.Length < 1)
                                return 0;

                            propertyLocalCopy.SetValue(instance, arguments[0].AsFloat(), null);

                            return 1;
                        });
                }
            }

            var methods =
                instance.GetType()
                    .GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof (ScriptingFunctionAttribute), true).Any());

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttributes(typeof (ScriptingFunctionAttribute), true).First() as ScriptingFunctionAttribute;

                var name = attribute.Name ?? method.Name;

                var parameters = method.GetParameters();
                var paramcount = parameters.Count();

                var outTypeCaster = (Func<object, int>) (o => 1);

                if (method.ReturnParameter.ParameterType == typeof (int))
                    outTypeCaster = o => (int)o;
                if (method.ReturnParameter.ParameterType == typeof(float))
                    outTypeCaster = o => Cell.FromFloat((float)o).AsInt32();
                else if (method.ReturnParameter.ParameterType == typeof (bool))
                    outTypeCaster = o => (bool) o ? 1 : 0;

                var methodLocalCopy = method;

                if (paramcount == 1 && parameters.First().ParameterType == typeof (AMXArgumentList))
                {
                    Register(name,
                        (amx, arguments) => outTypeCaster(methodLocalCopy.Invoke(instance, new object[] {arguments})));
                }
                else
                {
                    var inCasters = parameters.Select(CreateInTypeCaster).ToArray();
                    var outCasters = parameters.Select(CreateOutTypeCaster).ToArray();

                    Register(name, (amx, arguments) =>
                    {
                        if (arguments.Length != paramcount) return 0;

                        var parms = new object[paramcount];
                        for (var i = 0; i < paramcount; i++)
                            parms[i] = inCasters[i](arguments[i]);

                        var output = methodLocalCopy.Invoke(instance, parms);

                        for (var i = 0; i < paramcount; i++)
                            if (outCasters[i] != null)
                                outCasters[i](arguments[i], parms[i]);

                        return outTypeCaster(output);
                    });
                }
            }
        }

        #region native calls

        private class DefaultFunctions : IScriptingNatives
        {
            private Random _random;

            private static string Sprintf(string input, object[] inpVars)
            {
                var i = 0;
                input = Regex.Replace(input, "%.", m => ("{" + i++ + "}"));
                return string.Format(input, inpVars);
            }

            private static IEnumerable<char> FormatChars(string input)
            {
                for (int i = 0; i < input.Length - 1; i++)
                {
                    if (input[i] == '%')
                        yield return input[++i];
                }
            }

            [ScriptingFunction("frand")]
            public float FloatRandom()
            {
                if(_random == null) _random = new Random();

                return (float)_random.NextDouble();
            }
            [ScriptingFunction("log")]
            public void Log(string message)
            {
                Debug.WriteLine(message);
            }

            [ScriptingFunction("logf")]
            public bool LogFormat(AMXArgumentList arguments)
            {

                if (arguments.Length < 1)
                    return false;

                var format = arguments[0].AsString();
                int i = 1;
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


                Log(Sprintf(format, parms));

                return true;
            }
        }

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<string, Cell>> GetEnumerator()
        {
            return _publicVars.Select(n => new KeyValuePair<string, Cell>(n.Key, n.Value.Get())).GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}