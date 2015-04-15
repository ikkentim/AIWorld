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
using Microsoft.Xna.Framework;

namespace AIWorld
{
    internal class QuadTree : IEnumerable<IEntity>
    {
        private const int Capacity = 5;

        private readonly IEntity[] _entities = new IEntity[Capacity];

        private int _count;

        private QuadTree[] _parts;

        public QuadTree(AABB boundaries)
        {
            Boundaries = boundaries;
        }

        public AABB Boundaries { get; private set; }

        protected virtual IEnumerable<QuadTree> Parts
        {
            get { return _parts; }
        }

        public bool IsEmpty
        {
            get { return _count == 0 && _parts == null; }
        }

        public virtual IEnumerable<AABB> GetDebugBoxes()
        {
            if (Parts == null)
            {
                yield return Boundaries;
                yield break;
            }

            foreach (AABB a in Parts.SelectMany(p => p.GetDebugBoxes()))
                yield return a;
        }

        public virtual bool ContainsPoint(Vector3 point)
        {
            return Boundaries.ContainsPoint(point);
        }

        public virtual bool ContainsEntity(IEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            return _entities.Take(_count).Contains(entity) ||
                   (Parts != null && Parts.Any(p => p.ContainsEntity(entity)));
        }

        public virtual QuadTree GetQuadTreeContainingEntity(IEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            return _entities.Take(_count).Contains(entity)
                ? this
                : Parts == null
                    ? null
                    : Parts.Select(p => p.GetQuadTreeContainingEntity(entity)).FirstOrDefault(p => p != null);
        }

        protected virtual void Subdivide()
        {
            Vector3 half = Boundaries.HalfDimension/2;

            _parts = new[]
            {
                new QuadTree(new AABB(Boundaries.Center + new Vector3(half.X, half.Y, half.Z), half)),
                new QuadTree(new AABB(Boundaries.Center + new Vector3(half.X, half.Y, -half.Z), half)),
                new QuadTree(new AABB(Boundaries.Center + new Vector3(half.X, -half.Y, half.Z), half)),
                new QuadTree(new AABB(Boundaries.Center + new Vector3(half.X, -half.Y, -half.Z), half)),
                new QuadTree(new AABB(Boundaries.Center + new Vector3(-half.X, half.Y, half.Z), half)),
                new QuadTree(new AABB(Boundaries.Center + new Vector3(-half.X, half.Y, -half.Z), half)),
                new QuadTree(new AABB(Boundaries.Center + new Vector3(-half.X, -half.Y, half.Z), half)),
                new QuadTree(new AABB(Boundaries.Center + new Vector3(-half.X, -half.Y, -half.Z), half))
            };
        }

        public virtual IEnumerable<IEntity> Query(AABB box)
        {
            for (int i = 0; i < _count; i++)
                if (box.ContainsPoint(_entities[i].Position))
                    yield return _entities[i];

            if (Parts == null) yield break;

            foreach (IEntity e in Parts.Where(p => p.Boundaries.IntersectsWith(box)).SelectMany(p => p.Query(box)))
                yield return e;
        }

        protected virtual void DoInsert(IEntity entity)
        {
            if (_count < Capacity)
            {
                _entities[_count++] = entity;
                return;
            }

            if (Parts == null)
                Subdivide();

            Parts.First(p => p.ContainsPoint(entity.Position)).DoInsert(entity);
        }

        public void Insert(IEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (!ContainsPoint(entity.Position)) throw new ArgumentException("entity outside of boundaries");

            DoInsert(entity);
        }

        protected virtual QuadTree DoFindQuadTreeForEntity(IEntity entity)
        {
            if (_count != Capacity) return this;

            if (Parts == null) return this;

            if (_count == Capacity && _entities.Contains(entity))
                return this;

            return Parts.First(p => p.ContainsPoint(entity.Position)).DoFindQuadTreeForEntity(entity);
        }

        public QuadTree FindQuadTreeForEntity(IEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (!ContainsPoint(entity.Position)) throw new ArgumentException("entity outside of boundaries");

            return DoFindQuadTreeForEntity(entity);
        }

        protected virtual int DoRemove(IEntity entity)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_entities[i] != entity) continue;

                for (int j = i; j < _count - 1; j++)
                    _entities[j] = _entities[j + 1];

                _entities[--_count] = null;
                return _count;
            }

            if (Parts != null &&
                Parts.Where(p => p.ContainsEntity(entity)).Any(p => p.DoRemove(entity) == 0) &&
                _parts != null &&
                _parts.All(q => q.IsEmpty))
            {
                _parts = null;
            }
            return 0;
        }

        public void Remove(IEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            DoRemove(entity);
        }

        #region Implementation of IEnumerable

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IEntity> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
                yield return _entities[i];

            if (Parts == null) yield break;

            foreach (IEntity e in Parts.SelectMany(p => p).ToArray())
                yield return e;
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

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Boundaries + " ZZ" + (Boundaries.Center.Z + Boundaries.HalfDimension.Z);
        }

        #endregion
    }
}