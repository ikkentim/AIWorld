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

using System.Linq;
using AIWorld.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Entities
{
    public class WorldObject : Entity
    {
        private readonly float _angle;
        private readonly BasicEffect _basicEffect;
        private readonly ICameraService _cameraService;
        private readonly Model _model;
        private readonly bool _showDebugLines;
        private readonly Matrix[] _transforms;

        public WorldObject(Game game, string model, float size, Vector3 position, float angle, bool showDebugLines)
            : base(game)
        {
            Position = position;
            _model = game.Content.Load<Model>(model);
            _angle = angle;
            _showDebugLines = showDebugLines;
            Size = size;

            _transforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(_transforms);

            _cameraService = game.Services.GetService<ICameraService>();

            if (showDebugLines)
                _basicEffect = new BasicEffect(game.GraphicsDevice);
        }

        private void Line(Vector3 a, Vector3 b, Color c)
        {
            var vertices = new[] {new VertexPositionColor(a, c), new VertexPositionColor(b, c)};
            Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        #region Overrides of DrawableGameComponent

        public override void Draw(GameTime gameTime)
        {
            foreach (var mesh in _model.Meshes)
            {
                foreach (var effect in mesh.Effects.Cast<BasicEffect>())
                {
                    effect.World = _transforms[mesh.ParentBone.Index]*
                                   Matrix.CreateRotationY(_angle)*
                                   Matrix.CreateTranslation(Position);
                    effect.View = _cameraService.View;
                    effect.Projection = _cameraService.Projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }

            if (_showDebugLines)
            {
                _basicEffect.VertexColorEnabled = true;
                _basicEffect.World = Matrix.Identity;
                _basicEffect.View = _cameraService.View;
                _basicEffect.Projection = _cameraService.Projection;

                _basicEffect.CurrentTechnique.Passes[0].Apply();

                var szx = new Vector3(Size, 0, 0);
                var szz = new Vector3(0, 0, Size);
                Line(Position + szx, Position + szx + Vector3.Up, Color.Blue);
                Line(Position - szx, Position - szx + Vector3.Up, Color.Blue);
                Line(Position + szz, Position + szz + Vector3.Up, Color.Blue);
                Line(Position - szz, Position - szz + Vector3.Up, Color.Blue);
            }

            base.Draw(gameTime);
        }

        #endregion
    }
}