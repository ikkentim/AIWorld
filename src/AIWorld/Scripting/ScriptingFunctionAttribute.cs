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

namespace AIWorld.Scripting
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class ScriptingFunctionAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptingFunctionAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ScriptingFunctionAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptingFunctionAttribute" /> class.
        /// </summary>
        public ScriptingFunctionAttribute()
        {
        }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to ingore the setter of the property.
        /// </summary>
        public bool IngoreSetter { get; set; }
    }
}