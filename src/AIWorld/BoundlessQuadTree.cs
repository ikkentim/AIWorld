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
using System.Linq;
using Microsoft.Xna.Framework;

namespace AIWorld
{
    internal class BoundlessQuadTree : QuadTree
    {
        private const float Size = 20;

        #region Overrides of QuadTree

        protected override IEnumerable<QuadTree> Parts
        {
            get { return _parts; }
        }

        private static float GetValueForAxis(float value, float size)
        {
            if (value < 0)
            {
                float c;
                for (c = 0; c > value; c -= size) ;
                return c + size/2;
            }
            else
            {
                float c;
                for (c = 0; c < value; c += size) ;
                return c - size/2;
            }
        }

        protected override void DoInsert(IEntity entity)
        {
            QuadTree part = Parts.FirstOrDefault(p => p.ContainsPoint(entity.Position));

            if (part != null)
            {
                part.Insert(entity);
                return;
            }

            Vector3 vec = entity.Position;

            vec.X = GetValueForAxis(vec.X, Size);
            vec.Y = GetValueForAxis(vec.Y, Size);
            vec.Z = GetValueForAxis(vec.Z, Size);

            part = new QuadTree(new AABB(vec, new Vector3(Size)/2));
            _parts.Add(part);
            part.Insert(entity);
        }

        public override bool ContainsPoint(Vector3 point)
        {
            return true;
        }

        protected override void Subdivide()
        {
            throw new NotImplementedException("BoundlessQuadTree does not implement Subdivide");
        }

        #region Overrides of QuadTree

        protected override int DoRemove(IEntity entity)
        {
            QuadTree part = Parts.FirstOrDefault(p => p.ContainsEntity(entity));

            base.DoRemove(entity);

            Debug.Assert(part != null);

            if (part.IsEmpty)
            {
                _parts.Remove(part);
            }
            return 0;
        }

        #endregion

        #region Overrides of QuadTree

        protected override QuadTree DoFindQuadTreeForEntity(IEntity entity)
        {
            QuadTree part = Parts.FirstOrDefault(p => p.ContainsPoint(entity.Position));

            return part == null ? this : part.FindQuadTreeForEntity(entity);
        }

        #endregion

        #endregion

        private readonly List<QuadTree> _parts = new List<QuadTree>();

        public BoundlessQuadTree()
            : base(new AABB())
        {
        }
    }
}