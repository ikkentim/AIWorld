using System;
using System.Linq;
using AIWorld.Entities;
using AIWorld.Services;
using Microsoft.Xna.Framework;

namespace AIWorld.Steering
{
    class PursuitSteeringBehavior : ISteeringBehavior
    {
        private SeekSteeringBehavior _seek;
        private readonly Agent _agent;
        private readonly IGameWorldService _gameWorldService;
        private IMovingEntity _evader;

        public PursuitSteeringBehavior(Agent agent)
        {
            if (agent == null) throw new ArgumentNullException("agent");
            _agent = agent;
            _seek = new SeekSteeringBehavior(agent);
            _gameWorldService = agent.Game.Services.GetService<IGameWorldService>();
        }

        [SteeringBehaviorArgument]
        public int EvaderId
        {
            get { return _evader == null ? -1 : _evader.Id; }
            set { _evader = _gameWorldService.Entities.OfType<IMovingEntity>().FirstOrDefault(e => e.Id == value); }
        }

        #region Implementation of ISteeringBehavior

        public Vector3 Calculate(GameTime gameTime)
        {
            if(_evader == null) return Vector3.Zero;

            var toEvader = _evader.Position - _agent.Position;

            var relativeheading = Vector3.Dot(_agent.Heading, _evader.Heading);

            if(Vector3.Dot(toEvader, _agent.Heading) > 0 && relativeheading < -0.95f) //18 degrees
            {
                _seek.Target = _evader.Position;
                return _seek.Calculate(gameTime);
            }

            var lookAheadTime = toEvader.Length()/(_agent.MaxSpeed + _evader.Velocity.Length());

            _seek.Target = _evader.Position + _evader.Velocity*lookAheadTime;
            return _seek.Calculate(gameTime);
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return "Pursuit";
        }

        #endregion
    }
}