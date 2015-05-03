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
using AIWorld.Entities;
using Microsoft.Xna.Framework;

namespace AIWorld
{
    public class BoundlessQuadTree : QuadTree
    {
        private const float Size = 20;
        private readonly List<QuadTree> _parts = new List<QuadTree>();

        public BoundlessQuadTree()
            : base(new AABB())
        {
        }

        private static float GetQuadTreePositionForPosition(float value, float size)
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

        public void FixPositions()
        {
            foreach (var entity in Parts.SelectMany(part => part.RemoveEntitiesOutsideBoundaries(this).ToArray()).ToArray())
            {
                Add(entity);
            }
        }

        #region Overrides of QuadTree

        protected override IEnumerable<QuadTree> Parts
        {
            get { return _parts; }
        }

        public override IEnumerable<IEntity> RemoveEntitiesOutsideBoundaries(QuadTree partentTree)
        {
            return null;
        }

        public override void Add(IEntity entity)
        {
            var part = Parts.FirstOrDefault(p => p.ContainsPoint(entity.Position));

            if (part != null)
            {
                part.Add(entity);
                return;
            }

            var vec = entity.Position;

            vec.X = GetQuadTreePositionForPosition(vec.X, Size);
            vec.Y = GetQuadTreePositionForPosition(vec.Y, Size);
            vec.Z = GetQuadTreePositionForPosition(vec.Z, Size);

            part = new QuadTree(new AABB(vec, new Vector3(Size)/2));
            _parts.Add(part);
            part.Add(entity);
        }

        public override bool ContainsPoint(Vector3 point)
        {
            return true;
        }

        protected override void Subdivide()
        {
            throw new NotImplementedException("BoundlessQuadTree does not implement Subdivide");
        }

        public override int Remove(IEntity entity)
        {
            var part = Parts.FirstOrDefault(p => p.ContainsEntity(entity));

            base.Remove(entity);

            Debug.Assert(part != null);

            if (part.IsEmpty)
            {
                _parts.Remove(part);
            }
            return 0;
        }

        public override QuadTree FindQuadTreeForEntity(IEntity entity)
        {
            var part = Parts.FirstOrDefault(p => p.ContainsPoint(entity.Position));

            return part == null ? this : part.FindQuadTreeForEntity(entity);
        }

        #endregion
    }
}