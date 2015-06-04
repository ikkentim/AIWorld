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
using System.Linq;
using AIWorld.Helpers;
using AIWorld.Services;
using Microsoft.Xna.Framework;

namespace AIWorld.Entities
{
    public class Projectile : WorldObject
    {
        private readonly float _damageDrop;
        private readonly IGameWorldService _gameWorldService;
        private readonly float _lifeTime;
        private float _timeAlive;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WorldObject" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="caster">The caster.</param>
        /// <param name="modelName">The modelName.</param>
        /// <param name="damage">The damage.</param>
        /// <param name="lifeTime">The life time.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="translation">The translation.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="velocity">The velocity.</param>
        /// <param name="meshes">The meshes.</param>
        public Projectile(Game game, IEntity caster, string modelName, float damage, float lifeTime, Vector3 position,
            Vector3 rotation,
            Vector3 translation, Vector3 scale, Vector3 velocity, IEnumerable<string> meshes)
            : base(game, modelName, 0, position, rotation, translation, scale, false, meshes)
        {
            if (caster == null) throw new ArgumentNullException("caster");
            _gameWorldService = game.Services.GetService<IGameWorldService>();
            Caster = caster;
            Damage = damage;
            _damageDrop = (damage/2)/lifeTime;
            _lifeTime = lifeTime;
            Velocity = velocity;
            Heading = Vector3.Normalize(velocity);
            Side = Heading.RotateAboutOriginY(Vector3.Zero, MathHelper.ToRadians(90));
        }

        public Vector3 Velocity { get; private set; }
        public Vector3 Heading { get; private set; }
        public Vector3 Side { get; private set; }
        public float Damage { get; private set; }
        public IEntity Caster { get; private set; }

        #region Overrides of GameComponent

        public override void Update(GameTime gameTime)
        {
            _timeAlive += (float) gameTime.ElapsedGameTime.TotalSeconds;
            Damage -= _damageDrop*(float) gameTime.ElapsedGameTime.TotalSeconds;
            var oldPosition = Position;
            Position += Velocity*(float) gameTime.ElapsedGameTime.TotalSeconds;

            if (_timeAlive > _lifeTime)
            {
                _gameWorldService.Remove(this);
                return;
            }

            var len = (Position - oldPosition).Length();
            var ray = new Ray(new Vector3(oldPosition.X, 0, oldPosition.Z), Position - oldPosition);

            var collider =
                _gameWorldService.Entities
                    .Query(new AABB(oldPosition, new Vector3(len + MaxSize)))
                    .OfType<IHitable>()
                    .Where(h => h.IsSolid)
                    .Where(h => h != Caster)
                    .FirstOrDefault(h => ray.Intersects(new BoundingSphere(h.Position, h.Size)) != null && h.Hit(this));

            if (collider != null)
                _gameWorldService.Remove(this);

            base.Update(gameTime);
        }

        #endregion
    }
}