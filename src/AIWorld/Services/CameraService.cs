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
using System.Diagnostics;
using AIWorld.Entities;
using AIWorld.Helpers;
using AIWorld.Scripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace AIWorld.Services
{
    public class CameraService : GameComponent, ICameraService
    {
        public const float MaxCameraSpeed = 10.0f;
        public const float CameraSpeed = 500.0f;
        public const float DefaultZoom = 3;
        public const float MinZoom = 1;
        public const float MaxZoom = 15;
        public const float CameraTargetOffset = 0.35f;
        public const float CameraHeightOffset = 0.1f;
        private float _aspectRatio;
        private IEntity _target;
        private Vector3 _velocity;
        private Vector3 _undirectedVelocity;
        private float _zoom = DefaultZoom;
        private readonly AudioListener _audioListener;

        public CameraService(Game game) : base(game)
        {
            View = Matrix.CreateLookAt(Vector3.Zero, Vector3.Right, Vector3.Up);

            Projection = Matrix.Identity;
            _audioListener = new AudioListener {Up = Vector3.Up};

        }

        public Vector3 Position { get; set; }

        public float Zoom
        {
            get { return _zoom; }
        }

        public AudioListener AudioListener 
        { 
            get  { return _audioListener; } 
        }

        public float Rotation { get; private set; }
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }
        public Vector3 TargetPosition { get; set; }

        public override void Update(GameTime gameTime)
        {
            if (_aspectRatio != Game.GraphicsDevice.Viewport.AspectRatio)
            {
                _aspectRatio = Game.GraphicsDevice.Viewport.AspectRatio;
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), _aspectRatio, 0.1f,
                    10000.0f);
            }

            if (_target != null) TargetPosition = _target.Position;

            _velocity = ((TargetPosition - Position) * CameraSpeed).Truncate((TargetPosition - Position).Length());

            LimitVelocity(ref _undirectedVelocity, _zoom);
            LimitVelocity(ref _velocity, 15);

            _undirectedVelocity -= _undirectedVelocity*5.0f*(float) gameTime.ElapsedGameTime.TotalSeconds;

            if (_undirectedVelocity != Vector3.Zero)
            {
                var worldCameraVelocity = Vector3.Transform(_undirectedVelocity, Matrix.CreateRotationY(-Rotation));

                Position += worldCameraVelocity*(float) gameTime.ElapsedGameTime.TotalSeconds;
                TargetPosition = Position;
                _target = null;
            }
            else
            {
                Position += _velocity*(float) gameTime.ElapsedGameTime.TotalSeconds;
            }

            CalculateView();
            base.Update(gameTime);
        }

        private void LimitVelocity(ref Vector3 velocity, float zoom)
        {
            if (velocity == Vector3.Zero) return;

            if (velocity.Length() > MaxCameraSpeed * zoom)
            {
                velocity.Normalize();
                velocity *= MaxCameraSpeed * zoom;
            }

            if (velocity.LengthSquared() < 0.00001)
                velocity = Vector3.Zero;
        }

        public void SetTarget(IEntity target)
        {
            if ((_target = target) != null)
            {
                _undirectedVelocity = Vector3.Zero;
                TargetPosition = target.Position;
            }
        }

        public void SetTarget(Vector3 target)
        {
            _undirectedVelocity=Vector3.Zero;
            
            _target = null;
            TargetPosition = target;
        }

        public void AddVelocity(Vector3 acceleration)
        {
            _undirectedVelocity += acceleration * _zoom * 50;
        }

        public void Move(float deltaRotation, float deltaZoom)
        {
            _zoom += deltaZoom;
            Rotation += deltaRotation;

            _zoom = MathHelper.Clamp(_zoom + deltaZoom, MinZoom, MaxZoom);
        }

        private void CalculateView()
        {
            var realCameraTarget = Position + new Vector3(0, CameraTargetOffset, 0);
            var cameraPosition = realCameraTarget +
                                 new Vector3((float) Math.Cos(Rotation) * 2.8f,
                                     _zoom/3 + CameraTargetOffset + CameraHeightOffset,
                                     (float)Math.Sin(Rotation) * 2.8f) * _zoom;

            // Also update listener data
            _audioListener.Position = cameraPosition;
            _audioListener.Forward = Vector3.Normalize(realCameraTarget - cameraPosition);
      
            View = Matrix.CreateLookAt(cameraPosition, realCameraTarget, Vector3.Up);
        }
    }
}