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
using System.Linq;
using AIWorld.Events;
using AIWorld.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Entities
{
    /// <summary>
    ///     Represents a world object.
    /// </summary>
    public class WorldObject : Entity, IHitable
    {
        private readonly ICameraService _cameraService;
        private readonly ModelMesh[] _meshes;
        private readonly Matrix[] _transforms;

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
            Vector3 scale, bool isSolid, IEnumerable<string> meshes)
            : base(game)
        {
            ModelName = modelName;
            Position = position;
            var model = game.Content.Load<Model>(modelName);
            Rotation = rotation;
            Translation = translation;
            IsSolid = isSolid;
            Scale = scale;
            Size = size;
            _meshes = meshes.Any()
                ? model.Meshes.Where(n => meshes.Contains(n.Name)).ToArray()
                : model.Meshes.ToArray();

            _transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(_transforms);

            _cameraService = game.Services.GetService<ICameraService>();
        }

        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Translation { get; set; }

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
            var vpos = Vector3.Transform(Position, _cameraService.View);
            if (vpos.Z > 0)
                return;

            foreach (var mesh in _meshes)
            {
                foreach (var effect in mesh.Effects.Cast<BasicEffect>())
                {
                    effect.World =
                        _transforms[mesh.ParentBone.Index]*
                        (Matrix.CreateTranslation(Translation)* // Translation of the mesh
                         Matrix.CreateScale(Scale))*
                        (Matrix.CreateRotationX(Rotation.X)*
                         Matrix.CreateRotationY(Rotation.Y)*
                         Matrix.CreateRotationZ(Rotation.Z))*
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

        public bool Hit(Projectile projectile)
        {
            // Just drop the projectile. Keep object intact.
            return true;
        }

        #endregion
    }
}