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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld
{
    internal class House : IEntity
    {
        private readonly float _angle;
        private readonly Model _model;

        public House(Vector3 position, Model model, float angle)
        {
            Position = position;
            _model = model;
            _angle = angle;
            Size = 0.35f;
        }

        #region Implementation of IEntity

        public Vector3 Position { get; private set; }
        public float Size { get; set; }

        public void Update(GameWorld world, Matrix view, Matrix projection, GameTime gameTime)
        {
        }

        public void Render(GraphicsDevice graphicsDevice, Matrix view, Matrix projection, GameTime gameTime)
        {
            var transforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index]*Matrix.CreateRotationY(_angle)*
                                   Matrix.CreateTranslation(Position);
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }

        #endregion
    }
}