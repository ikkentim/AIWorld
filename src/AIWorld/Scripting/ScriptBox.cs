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
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using AMXWrapper;

namespace AIWorld.Scripting
{
    public class ScriptBox : AMX, IEnumerable<KeyValuePair<string, Cell>>
    {
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

            Register("log", Log);
            Register("logf", LogFormat);

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

        public void Register(IScriptingNatives natives)
        {
            if (natives == null) throw new ArgumentNullException("natives");
            natives.Register(this);
        }

        public void Register(AMXNativeFunction function)
        {
            if (function == null) throw new ArgumentNullException("function");
            if (function.Method.Name.Any(n => n == '<' || n == '>'))
                throw new ArgumentException("Invalid method name");

            Register(function.Method.Name, function);
        }

        #region native calls

        private int Log(AMX amx, AMXArgumentList arguments)
        {
            if (arguments.Length < 1)
                return 0;

            var msg = arguments[0].AsString();

            Debug.WriteLine(msg);

            return 1;
        }

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

        private int LogFormat(AMX amx, AMXArgumentList arguments)
        {

            if (arguments.Length < 1)
                return 0;

            var format = arguments[0].AsString();
            int i = 1;
            var parms = FormatChars(format).Select(c =>
            {
                switch (c)
                {
                    case 'd':
                    case 'i':
                        return (object)arguments[i++].AsCellPtr().Get().AsInt32();
                    case 's':
                        return (object) arguments[i++].AsString();
                    case 'f':
                        return (object)arguments[i++].AsCellPtr().Get().AsFloat();
                    case 'c':
                        return (char)arguments[i++].AsCellPtr().Get().AsInt32();
                    case 'x':
                        return string.Format("0x{0:X}", arguments[i++].AsCellPtr().Get().AsInt32());
                    case 'b':
                        return "0b" + Convert.ToString(arguments[i++].AsCellPtr().Get().AsInt32(), 2);
                    case '%':
                        return '%';
                    default:
                        i++;
                        return (object)0;
                }
            }).ToArray();


            Debug.WriteLine(Sprintf(format, parms));

            return 1;
        }
        #endregion

        #region Easy type register

        private object TypeCast(Type type, Cell cell)
        {
            if (type == typeof (string)) return cell.AsString();
            if (type == typeof (int)) return cell.AsInt32();
            if (type == typeof (float)) return cell.AsFloat();
            if (type == typeof (IntPtr)) return cell.AsIntPtr();
            if (type == typeof (CellPtr)) return cell.AsCellPtr();
            throw new ArgumentException("Invalid type " + type);
        }

        public void Register(Func<int> function)
        {
            if (function == null) throw new ArgumentNullException("function");
            var name = function.Method.Name;
            Register(name, (amx, arguments) =>
            {
                if (arguments.Length != 0) throw new ArgumentException(name + " accepts 0 argument");
                return (int) function.Method.Invoke(function.Target, null);
            });
        }

        public void Register<T1>(Func<T1, int> function)
        {
            if (function == null) throw new ArgumentNullException("function");
            var name = function.Method.Name;
            Register(name, (amx, arguments) =>
            {
                if (arguments.Length != 1) throw new ArgumentException(name + " accepts 1 argument");
                return (int) function.Method.Invoke(function.Target, new[]
                {
                    TypeCast(typeof (T1), arguments[0])
                });
            });
        }

        public void Register<T1, T2>(Func<T1, T2, int> function)
        {
            if (function == null) throw new ArgumentNullException("function");
            var name = function.Method.Name;
            Register(name, (amx, arguments) =>
            {
                if (arguments.Length != 2) throw new ArgumentException(name + " accepts 2 argument");
                return (int) function.Method.Invoke(function.Target, new[]
                {
                    TypeCast(typeof (T1), arguments[0]),
                    TypeCast(typeof (T2), arguments[1])
                });
            });
        }

        public void Register<T1, T2, T3>(Func<T1, T2, T3, int> function)
        {
            if (function == null) throw new ArgumentNullException("function");
            var name = function.Method.Name;
            Register(name, (amx, arguments) =>
            {
                if (arguments.Length != 3) throw new ArgumentException(name + " accepts 3 argument");
                return (int) function.Method.Invoke(function.Target, new[]
                {
                    TypeCast(typeof (T1), arguments[0]),
                    TypeCast(typeof (T2), arguments[1]),
                    TypeCast(typeof (T3), arguments[2])
                });
            });
        }

        public void Register<T1, T2, T3, T4>(Func<T1, T2, T3, T4, int> function)
        {
            if (function == null) throw new ArgumentNullException("function");
            var name = function.Method.Name;
            Register(name, (amx, arguments) =>
            {
                if (arguments.Length != 4) throw new ArgumentException(name + " accepts 4 argument");
                return (int) function.Method.Invoke(function.Target, new[]
                {
                    TypeCast(typeof (T1), arguments[0]),
                    TypeCast(typeof (T2), arguments[1]),
                    TypeCast(typeof (T3), arguments[2]),
                    TypeCast(typeof (T4), arguments[3])
                });
            });
        }

        public void Register<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, int> function)
        {
            if (function == null) throw new ArgumentNullException("function");
            var name = function.Method.Name;
            Register(name, (amx, arguments) =>
            {
                if (arguments.Length != 5) throw new ArgumentException(name + " accepts 5 argument");
                return (int) function.Method.Invoke(function.Target, new[]
                {
                    TypeCast(typeof (T1), arguments[0]),
                    TypeCast(typeof (T2), arguments[1]),
                    TypeCast(typeof (T3), arguments[2]),
                    TypeCast(typeof (T4), arguments[3]),
                    TypeCast(typeof (T5), arguments[4])
                });
            });
        }

        public void Register<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, int> function)
        {
            if (function == null) throw new ArgumentNullException("function");
            var name = function.Method.Name;
            Register(name, (amx, arguments) =>
            {
                if (arguments.Length != 6) throw new ArgumentException(name + " accepts 6 argument");
                return (int) function.Method.Invoke(function.Target, new[]
                {
                    TypeCast(typeof (T1), arguments[0]),
                    TypeCast(typeof (T2), arguments[1]),
                    TypeCast(typeof (T3), arguments[2]),
                    TypeCast(typeof (T4), arguments[3]),
                    TypeCast(typeof (T5), arguments[4]),
                    TypeCast(typeof (T6), arguments[5])
                });
            });
        }

        public void Register<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, int> function)
        {
            if (function == null) throw new ArgumentNullException("function");
            var name = function.Method.Name;
            Register(name, (amx, arguments) =>
            {
                if (arguments.Length != 7) throw new ArgumentException(name + " accepts 7 argument");
                return (int) function.Method.Invoke(function.Target, new[]
                {
                    TypeCast(typeof (T1), arguments[0]),
                    TypeCast(typeof (T2), arguments[1]),
                    TypeCast(typeof (T3), arguments[2]),
                    TypeCast(typeof (T4), arguments[3]),
                    TypeCast(typeof (T5), arguments[4]),
                    TypeCast(typeof (T6), arguments[5]),
                    TypeCast(typeof (T7), arguments[6])
                });
            });
        }

        public void Register<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, int> function)
        {
            if (function == null) throw new ArgumentNullException("function");
            var name = function.Method.Name;
            Register(name, (amx, arguments) =>
            {
                if (arguments.Length != 8) throw new ArgumentException(name + " accepts 8 argument");
                return (int) function.Method.Invoke(function.Target, new[]
                {
                    TypeCast(typeof (T1), arguments[0]),
                    TypeCast(typeof (T2), arguments[1]),
                    TypeCast(typeof (T3), arguments[2]),
                    TypeCast(typeof (T4), arguments[3]),
                    TypeCast(typeof (T5), arguments[4]),
                    TypeCast(typeof (T6), arguments[5]),
                    TypeCast(typeof (T7), arguments[6]),
                    TypeCast(typeof (T8), arguments[7])
                });
            });
        }

        public void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, int> function)
        {
            if (function == null) throw new ArgumentNullException("function");
            var name = function.Method.Name;
            Register(name, (amx, arguments) =>
            {
                if (arguments.Length != 9) throw new ArgumentException(name + " accepts 9 argument");
                return (int) function.Method.Invoke(function.Target, new[]
                {
                    TypeCast(typeof (T1), arguments[0]),
                    TypeCast(typeof (T2), arguments[1]),
                    TypeCast(typeof (T3), arguments[2]),
                    TypeCast(typeof (T4), arguments[3]),
                    TypeCast(typeof (T5), arguments[4]),
                    TypeCast(typeof (T6), arguments[5]),
                    TypeCast(typeof (T7), arguments[6]),
                    TypeCast(typeof (T8), arguments[7]),
                    TypeCast(typeof (T9), arguments[8])
                });
            });
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