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
using System.Diagnostics;
using System.Linq;
using AIWorld.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Entities
{
    public class WorldObject : Entity
    {
        private readonly BasicEffect _basicEffect;
        private readonly ICameraService _cameraService;
        private readonly Model _model;
        private readonly Vector3 _rotation;
        private readonly Vector3 _translation;
        private readonly Vector3 _scale;
        private readonly bool _showDebugLines;
        private ModelMesh[] _meshes;
        private Matrix[] _transforms;
        
        public WorldObject(Game game, string model, float size, Vector3 position, Vector3 rotation, Vector3 translation, Vector3 scale, IEnumerable<string> meshes, bool showDebugLines)
            : base(game)
        {
            Position = position;
            _model = game.Content.Load<Model>(model);
            _rotation = rotation;
            _translation = translation;
            _scale = scale;
            _showDebugLines = showDebugLines;
            Size = size;
            _meshes = meshes.Any()
                ? _model.Meshes.Where(n => meshes.Contains(n.Name)).ToArray()
                : _model.Meshes.ToArray();

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
            foreach (var mesh in _meshes)
            {
                foreach (var effect in mesh.Effects.Cast<BasicEffect>())
                {
                    effect.World =
                        _transforms[mesh.ParentBone.Index]*
                        (Matrix.CreateTranslation(_translation)*
                        Matrix.CreateScale(_scale))*
                        (Matrix.CreateRotationX(_rotation.X)*
                        Matrix.CreateRotationY(_rotation.Y)*
                        Matrix.CreateRotationZ(_rotation.Z))*
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

        #region Overrides of Entity

        public override bool OnClicked(MouseClickEventArgs e)
        {
            return false;
        }

        #endregion
    }
}