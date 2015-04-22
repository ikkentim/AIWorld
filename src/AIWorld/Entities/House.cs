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

using AIWorld.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Entities
{
    public class House : Entity
    {
        private readonly float _angle;
        private readonly Model _model;
        private readonly ICameraService _cameraService;

        public House(Vector3 position, Game game, float angle) : base(game)
        {
            Position = position;
            _model = game.Content.Load<Model>(@"models/house01");
            _angle = angle;
            Size = 0.35f;

            _cameraService = game.Services.GetService<ICameraService>();
        }

        #region Implementation of IEntity

        public override Vector3 Position { get; protected set; }
        public override float Size { get; protected set; }

        #endregion

        #region Overrides of DrawableGameComponent

        public override void Draw(GameTime gameTime)
        {
            var transforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index]*Matrix.CreateRotationY(_angle)*
                                   Matrix.CreateTranslation(Position);
                    effect.View = _cameraService.View;
                    effect.Projection = _cameraService.Projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }

        #endregion
    }
}