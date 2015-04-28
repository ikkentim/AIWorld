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

using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AIWorld
{
    public class Graph : Dictionary<Vector3, Node>
    {
        public void Add(Vector3 vector1, Vector3 vector2)
        {
            var node1 = ContainsKey(vector1) ? this[vector1] : this[vector1] = new Node(vector1);
            var node2 = ContainsKey(vector2) ? this[vector2] : this[vector2] = new Node(vector2);

            node1.Add(new Edge(node2, (vector2 - vector1).Length()));
        }
    }
}