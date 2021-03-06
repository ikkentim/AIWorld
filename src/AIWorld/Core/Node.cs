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
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace AIWorld.Core
{
    public class Node : List<Edge>, ICloneable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Node" /> class.
        /// </summary>
        /// <param name="position">The position.</param>
        public Node(Vector3 position)
        {
            Position = position;
        }

        public float Distance { get; set; }
        public Node Previous { get; set; }
        public Vector3 Position { get; private set; }

        public object Clone()
        {
            var node = new Node(Position) {Distance = Distance, Previous = Previous};
            node.AddRange(
                this.Select(
                    edge =>
                        new Edge(
                            new Node(edge.Target.Position)
                            {
                                Distance = edge.Target.Distance,
                                Previous = edge.Target.Previous == this ? node : null
                            },
                            edge.Distance)));
            return node;
        }
    }
}