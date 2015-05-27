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

namespace AIWorld.Helpers
{
    public class Pool<T> : IEnumerable<T> where T : class
    {
        private T[] _values = new T[1];

        public T this[int handle]
        {
            get { return handle < 0 || handle >= _values.Length ? null : _values[handle]; }
        }

        private int GetFreeHandle()
        {
            for (var i = 0; i < _values.Length; i++)
                if (_values[i] == null)
                    return i;

            var len = _values.Length;
            var tmp = new T[len*2];
            Array.Copy(_values, tmp, len);
            _values = tmp;

            return len;
        }

        public int Add(T value)
        {
            var handle = GetFreeHandle();

            _values[handle] = value;
            return handle;
        }

        public bool Remove(int handle)
        {
            if (handle < 0 || handle >= _values.Length || _values[handle] == null)
                return false;

            _values[handle] = null;

            return true;
        }

        #region Implementation of IEnumerable

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _values.Where(v => v != null).GetEnumerator();
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