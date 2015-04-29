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
        private const float MinimumDetectionBoxLength = 0.75f;
        private const float ArriveDecelerationTweaker = 1.3f;
        private const float AproxMaxObjectSize = 1.0f;
        private const float BreakingWeight = 0.005f;

        private readonly ScriptBox _script;
        private Model _model;
        private Matrix[] _transforms;
        private readonly ICameraService _cameraService;
        private readonly IGameWorldService _gameWorldService;

        private readonly Stack<Vector3> _path = null; 
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
            _gameWorldService = game.Services.GetService<IGameWorldService>();

            // simple default route for testing
            var target = new Vector3(10, 0, 10);
            var a = _gameWorldService.Graph.NearestNode(Vector3.Zero);
            var b = _gameWorldService.Graph.NearestNode(target);
            _path = new Stack<Vector3>(new[] { target }.Concat(_gameWorldService.Graph.ShortestPath(a, b)));
        }

        #region Steering behaviour

        private float DetectionBoxLength
        {
            get { return MinimumDetectionBoxLength + (Velocity.Length() / MaxSpeed) * MinimumDetectionBoxLength; }
        }

        private Vector3 Seek(Vector3 target)
        {
            return (target - Position).Truncate(MaxSpeed) - Velocity;
        }

        private Vector3 Arrive(Vector3 target)
        {
            var toTarget = target - Position;
            var distance = toTarget.Length();

            if (distance > 0.00001)
            {
                var speed = distance / (ArriveDecelerationTweaker);
                speed = Math.Min(speed, MaxSpeed);
                var desiredVelocity = toTarget * speed / distance;

                return desiredVelocity - Velocity;
            }

            return Vector3.Zero;
        }

        private Vector3 AvoidObstacles()
        {
            var bLength = DetectionBoxLength;

            var entities =
                _gameWorldService.Entities.Query(new AABB(Position, new Vector3(bLength + AproxMaxObjectSize)))
                    .Where(e => e != this && (e.Position - Position).Length() < e.Size + bLength);

            IEntity closest = null;
            var closestDistance = float.MaxValue;
            var localPositionOfClosestPoint = Vector3.Zero;

            foreach (var e in entities)
            {
                var localPoint = Transform.ToLocalSpace(Position, Heading, Vector3.Up, Side, e.Position);
                if (localPoint.X > 0)
                {
                    var combinedSize = e.Size + Size;

                    if (Math.Abs(localPoint.Z) < combinedSize)
                    {
                        var sqrtpart = (float) Math.Sqrt(combinedSize*combinedSize - localPoint.Z*localPoint.Z);

                        var ip = sqrtpart <= localPoint.X ? localPoint.X - sqrtpart : localPoint.X + sqrtpart;

                        if (ip < closestDistance)
                        {
                            closestDistance = ip;
                            closest = e;
                            localPositionOfClosestPoint = localPoint;
                        }
                    }
                }
            }

            if (closest == null)
                return Vector3.Zero;

            var multiplier = 1 + (bLength - localPositionOfClosestPoint.X)/bLength;

            return
                Transform.VectorToWorldSpace(Heading, Vector3.Up, Side,
                    new Vector3((closest.Size - localPositionOfClosestPoint.X)*BreakingWeight, 0,
                        closest.Size - localPositionOfClosestPoint.Z*multiplier));
        }

        private Vector3 CalculateSteeringForce()
        {
            if (_path == null || !_path.Any()) return Vector3.Zero;

            var target = _path.Peek();
            var force = Vector3.Zero;

            if (_path.Count > 1)
                force += Seek(target) * 0.9f;
            else
                force += Arrive(target) * 0.9f;

            force += AvoidObstacles() * 1.6f;

            return force.Truncate(MaxForce);
        }

        #endregion

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
            Vector3 steeringForce = CalculateSteeringForce();

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

        private void UpdateTarget()
        {
            if (_path == null || !_path.Any()) return;
            if (_path.Count == 1)
            {
                if (Velocity.LengthSquared() < 0.001)
                {
                    Velocity = Vector3.Zero;
                    _path.Pop();

                    try
                    {
                        _script.FindPublic("OnPathEnd").Execute();
                    }
                    catch (AMXException)
                    {
                    }
                }
            }
            else if ((Position - _path.Peek()).LengthSquared() < 0.9f) _path.Pop();
        }

        #region Overrides of GameComponent

        public override void Update(GameTime gameTime)
        {
            UpdatePosition(gameTime);
            UpdateTarget();

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
