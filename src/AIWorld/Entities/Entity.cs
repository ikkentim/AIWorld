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

using AIWorld.Events;
using Microsoft.Xna.Framework;

namespace AIWorld.Entities
{
    /// <summary>
    ///     Represents an entity.
    /// </summary>
    public abstract class Entity : DrawableGameComponent, IEntity
    {
        public const float MaxSize = 4;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Entity" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        protected Entity(Game game) : base(game)
        {
        }

        #region Implementation of IEntity

        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        ///     Gets the position.
        /// </summary>
        public virtual Vector3 Position { get; set; }

        /// <summary>
        ///     Gets the size.
        /// </summary>
        public virtual float Size { get; set; }

        /// <summary>
        ///     Is called when the user has clicked on this instance.
        /// </summary>
        /// <param name="e">The <see cref="MouseClickEventArgs" /> instance containing the event data.</param>
        /// <returns>
        ///     True if this instance has handled the input; False otherwise.
        /// </returns>
        public abstract bool OnClicked(MouseClickEventArgs e);

        #endregion
    }
}