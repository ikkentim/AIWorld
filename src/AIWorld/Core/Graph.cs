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
using AIWorld.Helpers;
using Microsoft.Xna.Framework;

namespace AIWorld.Core
{
    public class Graph : Dictionary<Vector3, Node>
    {
        private readonly Dictionary<Tuple<Vector3, Vector3>, IEnumerable<Node>> _pathCache =
            new Dictionary<Tuple<Vector3, Vector3>, IEnumerable<Node>>();

        public void Add(Vector3 vector1, Vector3 vector2)
        {
            var node1 = ContainsKey(vector1) ? this[vector1] : this[vector1] = new Node(vector1);
            var node2 = ContainsKey(vector2) ? this[vector2] : this[vector2] = new Node(vector2);

            node1.Add(new Edge(node2, (vector2 - vector1).Length()));
        }

        public Vector3 NearestNode(Vector3 point)
        {
            var distance = float.PositiveInfinity;

            var node = Vector3.Zero;

            foreach (var k in Keys)
            {
                var d = (k - point).LengthSquared();
                if (!(d < distance)) continue;

                node = k;
                distance = d;
            }

            if (float.IsPositiveInfinity(distance))
                throw new Exception("graph is empty");
            return node;
        }

        public IEnumerable<Node> ShortestPath(Vector3 start, Vector3 finish)
        {
            return ShortestPathAStar(start, finish);
        }

        public IEnumerable<Node> ShortestPathAStar(Vector3 start, Vector3 finish)
        {
            IEnumerable<Node> cached;

            if (_pathCache.TryGetValue(new Tuple<Vector3, Vector3>(start, finish), out cached))
                return cached;

            var nodes = new List<Node>();
            var result = new List<Node>();

            nodes.Add(Values.FirstOrDefault(n => n.Position == start));

            foreach (var n in Values)
            {
                n.Distance = float.PositiveInfinity;
            }

            this[start].Distance = 0;
            this[start].Previous = null;

            while (nodes.Count != 0)
            {
                var current = nodes.MinBy(n => n.Distance + (n.Position - finish).ManhattanLength());

                // Start and end nodes are in different graphs
                if (float.IsPositiveInfinity(current.Distance)) return result;

                nodes.Remove(current);

                if (current.Position == finish)
                {
                    while (current.Previous != null)
                    {
                        result.Add(current.Clone() as Node);
                        current = current.Previous;
                    }

                    return result;
                }

                foreach (var edge in current)
                {
                    var alt = current.Distance + edge.Distance;

                    if (alt < edge.Target.Distance)
                    {
                        edge.Target.Distance = alt;
                        edge.Target.Previous = current;
                        nodes.Add(edge.Target);
                    }
                }
            }

            return result;
        }

        private IEnumerable<Node> ShortestPathDijkstra(Vector3 start, Vector3 finish)
        {
            var nodes = new List<Node>(Values);

            foreach (var n in Values) n.Distance = float.PositiveInfinity;

            this[start].Distance = 0;
            this[start].Previous = null;

            while (nodes.Count != 0)
            {
                var minDistance = nodes.Min(n => n.Distance);

                // Start and end nodes are in different graphs
                if (float.IsPositiveInfinity(minDistance)) yield break;

                var smallest = nodes.First(n => n.Distance == minDistance);
                nodes.Remove(smallest);

                if (smallest.Position == finish)
                {
                    while (smallest.Previous != null)
                    {
                        yield return smallest;
                        smallest = smallest.Previous;
                    }

                    yield break;
                }

                foreach (var neighbor in smallest)
                {
                    var alt = smallest.Distance + neighbor.Distance;
                    if (alt < neighbor.Target.Distance)
                    {
                        neighbor.Target.Distance = alt;
                        neighbor.Target.Previous = smallest;
                    }
                }
            }
        }
    }
}