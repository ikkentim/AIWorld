using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AIWorld.Helpers;
using AIWorld.Services;
using AMXWrapper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld.Entities
{
    public class Agent : Entity, IMovingEntity
    {
        private readonly ScriptBox _script;
        private Model _model;
        private Matrix[] _transforms;
        private readonly ICameraService _cameraService;

        public Agent(Game game, string scriptname, Vector3 position)
            : base(game)
        {
            if (scriptname == null) throw new ArgumentNullException("scriptname");
            Position = position;
    
            _script = new ScriptBox("agent", scriptname);
            _script.Register<string>(SetModel);
            _script.Register<float>(SetSize);
            _script.Register<float>(SetMaxForce);
            _script.Register<float>(SetMaxSpeed);
            _script.Register<float>(SetMass);
            _script.ExecuteMain();

            _cameraService = game.Services.GetService<ICameraService>();
        }

        #region scripting natives

        private int SetMass(float mass)
        {
            Mass = mass;
            return 1;
        }
        private int SetMaxForce(float maxForce)
        {
            MaxForce = maxForce;
            return 1;
        }
        private int SetMaxSpeed(float maxSpeed)
        {
            MaxSpeed = maxSpeed;
            return 1;
        }

        private int SetModel(string modelname)
        {
            _model = Game.Content.Load<Model>(modelname);

            _transforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(_transforms);
            return 1;
        }

        private int SetSize(float size)
        {
            Size = size;
            return 1;
        }

        #endregion

        private void UpdatePosition(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 steeringForce = Vector3.Zero;// CalculateSteeringForce();

            Vector3 acceleration = steeringForce / Mass;
            Velocity += acceleration * deltaTime;

            Velocity = Velocity.Truncate(MaxSpeed);

            Position += Velocity * deltaTime;

            if (Velocity.LengthSquared() > 0.00001)
            {
                Heading = Vector3.Normalize(Velocity);
                Side = Heading.RotateAboutOriginY(Vector3.Zero, MathHelper.ToRadians(90));
            }
        }

        #region Overrides of GameComponent

        public override void Update(GameTime gameTime)
        {
            UpdatePosition(gameTime);

            base.Update(gameTime);
        }

        #endregion

        #region Overrides of DrawableGameComponent

        public override void Draw(GameTime gameTime)
        {
            if (_model != null)
            {
                foreach (var mesh in _model.Meshes)
                {
                    foreach (var effect in mesh.Effects.Cast<BasicEffect>())
                    {
                        effect.World = _transforms[mesh.ParentBone.Index]*Matrix.CreateRotationY(Heading.GetYAngle())*
                                       Matrix.CreateTranslation(Position);
                        effect.View = _cameraService.View;
                        effect.Projection = _cameraService.Projection;
                        effect.EnableDefaultLighting();
                    }
                    mesh.Draw();
                }
            }
            base.Draw(gameTime);
        }

        #endregion

        #region Implementation of IMovingEntity

        public Vector3 Velocity { get; private set; }
        public float Mass { get; private set; }
        public Vector3 Heading { get; private set; }
        public Vector3 Side { get; private set; }
        public float MaxSpeed { get; private set; }
        public float MaxForce { get; private set; }
        public float MaxTurnRate { get; private set; }

        #endregion
    }
}
