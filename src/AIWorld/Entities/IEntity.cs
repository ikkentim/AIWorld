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
using Microsoft.Xna.Framework;

namespace AIWorld.Entities
{
    /// <summary>
    ///     Contains methods for an entity.
    /// </summary>
    public interface IEntity : IGameComponent, IDisposable
    {
        /// <summary>
        ///     Gets the position.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        ///     Gets the size.
        /// </summary>
        float Size { get; }

        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        int Id { get; set; }

        /// <summary>
        ///     Is called when the user has clicked on this instance.
        /// </summary>
        /// <param name="e">The <see cref="MouseClickEventArgs" /> instance containing the event data.</param>
        /// <returns>True if this instance has handled the input; False otherwise.</returns>
        bool OnClicked(MouseClickEventArgs e);
    }
}