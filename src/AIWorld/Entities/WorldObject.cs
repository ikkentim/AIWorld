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

using System.Collections.Generic;
using System.Linq;
using AIWorld.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Entities
{
    /// <summary>
    ///     Represents a world object.
    /// </summary>
    public class WorldObject : Entity
    {
        private readonly ICameraService _cameraService;
        private readonly ModelMesh[] _meshes;
        private readonly Vector3 _rotation;
        private readonly Vector3 _scale;
        private readonly Matrix[] _transforms;
        private readonly Vector3 _translation;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WorldObject" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="modelName">The modelName.</param>
        /// <param name="size">The size.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="translation">The translation.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="meshes">The meshes.</param>
        public WorldObject(Game game, string modelName, float size, Vector3 position, Vector3 rotation,
            Vector3 translation,
            Vector3 scale, IEnumerable<string> meshes)
            : base(game)
        {
            ModelName = modelName;
            Position = position;
            var model = game.Content.Load<Model>(modelName);
            _rotation = rotation;
            _translation = translation;
            _scale = scale;
            Size = size;
            _meshes = meshes.Any()
                ? model.Meshes.Where(n => meshes.Contains(n.Name)).ToArray()
                : model.Meshes.ToArray();

            _transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(_transforms);

            _cameraService = game.Services.GetService<ICameraService>();
        }

        /// <summary>
        ///     Gets the model name.
        /// </summary>
        public string ModelName { get; private set; }

        #region Overrides of DrawableGameComponent

        /// <summary>
        ///     Draws this instance.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Draw(GameTime gameTime)
        {
            foreach (var mesh in _meshes)
            {
                foreach (var effect in mesh.Effects.Cast<BasicEffect>())
                {
                    effect.World =
                        _transforms[mesh.ParentBone.Index]*
                        (Matrix.CreateTranslation(_translation)* // Translation of the mesh
                         Matrix.CreateScale(_scale))*
                        (Matrix.CreateRotationX(_rotation.X)*
                         Matrix.CreateRotationY(_rotation.Y)*
                         Matrix.CreateRotationZ(_rotation.Z))*
                        Matrix.CreateTranslation(Position); // Translation of the model in the world space
                    effect.View = _cameraService.View;
                    effect.Projection = _cameraService.Projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }

        #endregion

        #region Overrides of Entity

        /// <summary>
        ///     Is called when the user has clicked on this instance.
        /// </summary>
        /// <param name="e">The <see cref="MouseClickEventArgs" /> instance containing the event data.</param>
        /// <returns>
        ///     True if this instance has handled the input; False otherwise.
        /// </returns>
        public override bool OnClicked(MouseClickEventArgs e)
        {
            return false;
        }

        #endregion
    }
}