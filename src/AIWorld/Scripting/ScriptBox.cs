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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AMXWrapper;
using Microsoft.Xna.Framework;

namespace AIWorld.Scripting
{
    public class ScriptBox : AMX, IEnumerable<KeyValuePair<string, Cell>>
    {
        private static readonly DefaultFunctions DefaultFunctions = new DefaultFunctions();
        private readonly Dictionary<string, AMXPublic> _publics = new Dictionary<string, AMXPublic>();
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

            Register(DefaultFunctions);

            // Prepare public vars table
            for (var i = 0; i < PublicVarCount; i++)
            {
                string varname;
                var ptr = GetPublicVar(i, out varname);
                _publicVars[varname] = ptr;
            }

            for (var i = 0; i < PublicCount; i++)
            {
                string publicname;
                GetPublic(i, out publicname);
                _publics[publicname] = new AMXPublic(this, i);
            }
        }

        public IDictionary<string, AMXPublic> Publics
        {
            get { return _publics; }
        }

        public IDictionary<string, CellPtr> PublicVars
        {
            get { return _publicVars; }
        }

        public void Register(AMXNativeFunction function)
        {
            if (function == null) throw new ArgumentNullException("function");
            if (function.Method.Name.Any(n => n == '<' || n == '>'))
                throw new ArgumentException("Invalid method name");

            Register(function.Method.Name, function);
        }

        private static Func<Cell, object> CreateInTypeCaster(ParameterInfo info)
        {
            if (info.ParameterType.IsByRef) return cell => null;

            if (info.ParameterType.IsEnum && info.ParameterType.GetEnumUnderlyingType() == typeof (int))
                return cell => Enum.ToObject(info.ParameterType, cell.AsInt32());

            if (info.ParameterType == typeof(bool)) return cell => cell.AsInt32() != 0;
            if (info.ParameterType == typeof (string)) return cell => cell.AsString();
            if (info.ParameterType == typeof (int)) return cell => cell.AsInt32();
            if (info.ParameterType == typeof (uint)) return cell => cell.AsUInt32();
            if (info.ParameterType == typeof (float)) return cell => cell.AsFloat();
            if (info.ParameterType == typeof (IntPtr)) return cell => cell.AsIntPtr();
            if (info.ParameterType == typeof (CellPtr)) return cell => cell.AsCellPtr();

            throw new ArgumentException("Invalid argument type " + info.ParameterType);
        }

        private static Action<Cell, object> CreateOutTypeCaster(ParameterInfo info)
        {
            if (!info.ParameterType.IsByRef) return null;

            if (info.ParameterType.GetElementType() == typeof (int))
                return (cell, value) => { cell.AsCellPtr().Set((int) value); };
            if (info.ParameterType.GetElementType() == typeof (float))
                return (cell, value) => { cell.AsCellPtr().Set(Cell.FromFloat((float) value)); };
            throw new ArgumentException("Invalid argument type " + info.ParameterType);
        }

        public void Register(params IScriptingNatives[] instances)
        {
            if (instances == null) throw new ArgumentNullException("instances");

            foreach (var instance in instances)
            {
                var instanceLocalCopy = instance;

                var properties =
                    instance.GetType()
                        .GetProperties()
                        .Where(m => m.GetCustomAttributes(typeof (ScriptingFunctionAttribute), true).Any());

                foreach (var property in properties)
                {
                    var attribute =
                        property.GetCustomAttributes(typeof (ScriptingFunctionAttribute), true).First() as
                            ScriptingFunctionAttribute;

                    // Indexers are illegal
                    if (property.GetIndexParameters().Length > 0) continue;

                    var name = attribute.Name ?? property.Name;

                    var propertyLocalCopy = property;

                    if (property.PropertyType == typeof (Vector3))
                    {
                        Register(string.Format("Get{0}", name), (amx, arguments) =>
                        {
                            if (arguments.Length < 2)
                                return 0;

                            var value = (Vector3) propertyLocalCopy.GetValue(instanceLocalCopy, null);

                            arguments[0].AsCellPtr().Set(Cell.FromFloat(value.X));
                            arguments[1].AsCellPtr().Set(Cell.FromFloat(value.Z));
                            return 1;
                        });

                        if (property.GetSetMethod() != null && !attribute.IngoreSetter)
                            Register(string.Format("Set{0}", name), (amx, arguments) =>
                            {
                                if (arguments.Length < 2)
                                    return 0;

                                propertyLocalCopy.SetValue(instanceLocalCopy,
                                    new Vector3(arguments[0].AsFloat(), 0, arguments[1].AsFloat()), null);

                                return 1;
                            });
                    }
                    else if (property.PropertyType == typeof (string))
                    {
                        Register(string.Format("Get{0}", name), (amx, arguments) =>
                        {
                            if (arguments.Length < 2)
                                return -1;

                            var length = arguments[1].AsInt32() - 1;
                            if (length <= 0) return -1;

                            var value = propertyLocalCopy.GetValue(instanceLocalCopy, null) as string;
                            if (value == null) return -1;

                            SetString(arguments[0].AsCellPtr(),
                                value.Length > length ? value.Substring(0, length) : value,
                                false);

                            return value.Length;
                        });

                        if (property.GetSetMethod() != null && !attribute.IngoreSetter)
                            Register(string.Format("Set{0}", name), (amx, arguments) =>
                            {
                                if (arguments.Length < 1)
                                    return 0;

                                propertyLocalCopy.SetValue(instanceLocalCopy, arguments[0].AsString(), null);

                                return 1;
                            });
                    }
                    else if (property.PropertyType == typeof (int))
                    {
                        Register(string.Format("Get{0}", name),
                            (amx, arguments) => (int) propertyLocalCopy.GetValue(instanceLocalCopy, null));

                        if (property.GetSetMethod() != null && !attribute.IngoreSetter)
                            Register(string.Format("Set{0}", name), (amx, arguments) =>
                            {
                                if (arguments.Length < 1)
                                    return 0;

                                propertyLocalCopy.SetValue(instanceLocalCopy, arguments[0].AsInt32(), null);

                                return 1;
                            });
                    }
                    else if (property.PropertyType == typeof (bool))
                    {
                        Register(string.Format("Get{0}", name),
                            (amx, arguments) => (bool) propertyLocalCopy.GetValue(instanceLocalCopy, null) ? 1 : 0);

                        if (property.GetSetMethod() != null && !attribute.IngoreSetter)
                            Register(string.Format("Set{0}", name), (amx, arguments) =>
                            {
                                if (arguments.Length < 1)
                                    return 0;

                                propertyLocalCopy.SetValue(instanceLocalCopy, arguments[0].AsInt32() != 0, null);

                                return 1;
                            });
                    }
                    else if (property.PropertyType == typeof (float))
                    {
                        Register(string.Format("Get{0}", name),
                            (amx, arguments) =>
                                Cell.FromFloat((float) propertyLocalCopy.GetValue(instanceLocalCopy, null)).AsInt32());

                        if (property.GetSetMethod() != null && !attribute.IngoreSetter)
                            Register(string.Format("Set{0}", name), (amx, arguments) =>
                            {
                                if (arguments.Length < 1)
                                    return 0;

                                propertyLocalCopy.SetValue(instanceLocalCopy, arguments[0].AsFloat(), null);

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
                    var attribute =
                        method.GetCustomAttributes(typeof (ScriptingFunctionAttribute), true).First() as
                            ScriptingFunctionAttribute;

                    var name = attribute.Name ?? method.Name;

                    var parameters = method.GetParameters();
                    var paramcount = parameters.Count();

                    var outTypeCaster = (Func<object, int>) (o => 1);

                    if (method.ReturnParameter.ParameterType == typeof (int))
                        outTypeCaster = o => (int) o;
                    if (method.ReturnParameter.ParameterType == typeof (float))
                        outTypeCaster = o => Cell.FromFloat((float) o).AsInt32();
                    else if (method.ReturnParameter.ParameterType == typeof (bool))
                        outTypeCaster = o => (bool) o ? 1 : 0;

                    var methodLocalCopy = method;

                    if (paramcount == 1 && parameters.First().ParameterType == typeof (AMXArgumentList))
                    {
                        Register(name,
                            (amx, arguments) =>
                                outTypeCaster(methodLocalCopy.Invoke(instanceLocalCopy, new object[] {arguments})));
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

                            var output = methodLocalCopy.Invoke(instanceLocalCopy, parms);

                            for (var i = 0; i < paramcount; i++)
                                if (outCasters[i] != null)
                                    outCasters[i](arguments[i], parms[i]);

                            return outTypeCaster(output);
                        });
                    }
                }
            }
        }

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