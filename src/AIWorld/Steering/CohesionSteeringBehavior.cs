using System;
using System.Linq;
using System.Security.AccessControl;
using AIWorld.Entities;
using AIWorld.Services;
using Microsoft.Xna.Framework;

namespace AIWorld.Steering
{
    public abstract class CohesionSteeringBehavior : ISteeringBehavior
    {
        private readonly Agent _agent;
        private readonly SeekSteeringBehavior _seek;
        private readonly IGameWorldService _gameWorldService;
        private float _range;
        private float _rangeSquared;

        protected CohesionSteeringBehavior(Agent agent)
        {
            if (agent == null) throw new ArgumentNullException("agent");
            _agent = agent;
            _seek = new SeekSteeringBehavior(agent);
            _gameWorldService = agent.Game.Services.GetService<IGameWorldService>();
        }

        [SteeringBehaviorArgument(0)]
        public float Range
        {
            get { return _range; }
            set
            {
                _range = value;
                _rangeSquared = value * value;
            }
        }

        [SteeringBehaviorArgument(1)]
        public string Key { get; set; }

        public object KeyValue { get; protected set; }


        #region Implementation of ISteeringBehavior

        public Vector3 Calculate(GameTime gameTime)
        {
            if (KeyValue == null) return Vector3.Zero;

            Vector3 centerOfMass = Vector3.Zero;
            int count = 0;

            foreach (var agent in _gameWorldService.Entities.Query(new AABB(_agent.Position, new Vector3(Range)))
                .OfType<Agent>()
                .Where(a => a != _agent)
                .Where(a => KeyValue.Equals(a.GetVarObject(Key)))
                .Where(a => Vector3.DistanceSquared(_agent.Position, a.Position) < _rangeSquared))
            {
                centerOfMass += agent.Position;
                count++;
            }

            if (count <= 0) return Vector3.Zero;

            centerOfMass /= count;
            centerOfMass -= _agent.Heading;

            _seek.Target = centerOfMass;
            return Vector3.Normalize(_seek.Calculate(gameTime));
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
            return "Cohesion";
        }

        #endregion
    }
}