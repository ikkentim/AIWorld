using System;
using System.Linq;
using AIWorld.Entities;
using AIWorld.Helpers;
using AIWorld.Services;
using Microsoft.Xna.Framework;

namespace AIWorld.Steering
{
    class OffsetPursuitSteeringBehavior : ISteeringBehavior
    {
        private readonly Agent _agent;
        private ArriveSteeringBehavior _arrive;
        private IMovingEntity _leader;
        private IGameWorldService _gameWorldService;

        public OffsetPursuitSteeringBehavior(Agent agent)
        {
            if (agent == null) throw new ArgumentNullException("agent");

            _arrive = new ArriveSteeringBehavior(agent);
            _gameWorldService = agent.Game.Services.GetService<IGameWorldService>();
            _agent = agent;
        }

        [SteeringBehaviorArgument(0)]
        public int LeaderId
        {
            get { return _leader == null ? -1 : _leader.Id; }
            set { _leader = _gameWorldService.Entities.OfType<IMovingEntity>().FirstOrDefault(e => e.Id == value); }
        }
        [SteeringBehaviorArgument(1)]
        public Vector3 Offset { get; set; }

        #region Implementation of ISteeringBehavior

        public Vector3 Calculate(GameTime gameTime)
        {
            if (_leader == null)
                return Vector3.Zero;
            
            var worldOffsetPos = Transform.PointToWorldSpace(_leader.Position, _leader.Heading, Vector3.Up, _leader.Side,
                Offset);

            var toOffset = worldOffsetPos - _agent.Position;

            var lookAheadTime = toOffset.Length()/(_agent.MaxSpeed + _leader.Velocity.Length());

            _arrive.Target = worldOffsetPos + _leader.Velocity*lookAheadTime;

            return _arrive.Calculate(gameTime);
        }

        #endregion
    }
}