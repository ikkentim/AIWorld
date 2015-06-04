using System;
using System.Linq;
using AIWorld.Entities;
using AIWorld.Services;
using Microsoft.Xna.Framework;

namespace AIWorld.Steering
{
    public abstract class AlignmentSteeringBehavior : ISteeringBehavior
    {
        private readonly Agent _agent;
        private readonly IGameWorldService _gameWorldService;
        private float _range;
        private float _rangeSquared;
        private Vector3 _calculated;

        protected AlignmentSteeringBehavior(Agent agent)
        {
            if (agent == null) throw new ArgumentNullException("agent");
            _agent = agent;
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

            Vector3 avgHeading = Vector3.Zero;
            int count = 0;

            foreach (var agent in _gameWorldService.Entities.Query(new AABB(_agent.Position, new Vector3(Range)))
                .OfType<Agent>()
                //.Where(a => a != _agent)
                .Where(a => KeyValue.Equals(a.GetVarObject(Key)))
                .Where(a => Vector3.DistanceSquared(_agent.Position, a.Position) < _rangeSquared))
            {
                avgHeading += agent.Velocity;
                count++;
            }

            if (count <= 0) return Vector3.Zero;

            avgHeading /= count;
            avgHeading -= _agent.Velocity;

            return _calculated = avgHeading;
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
            return string.Format("Alignment ({0},{1})", _calculated.X, _calculated.Z);
        }

        #endregion
    }
}