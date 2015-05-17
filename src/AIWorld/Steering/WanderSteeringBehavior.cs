using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIWorld.Entities;
using AIWorld.Helpers;
using Microsoft.Xna.Framework;

namespace AIWorld.Steering
{
    public class WanderSteeringBehavior : ISteeringBehavior
    {
        private readonly float _jitter;
        private readonly float _radius;
        private readonly float _distance;
        Random _random = new Random();

        private Vector3 wanderTarget;
        public Agent Agent { get; private set; }

        public WanderSteeringBehavior(Agent agent, float jitter, float radius, float distance)
        {
            _jitter = jitter;
            _radius = radius;
            _distance = distance;
            Agent = agent;

            var theta = (float)_random.NextDouble() * (float)Math.PI * 2;
            wanderTarget = new Vector3(_radius * (float)Math.Cos(theta), 0,
                _radius * (float)Math.Sin(theta));
        }

        #region Implementation of ISteeringBehavior

        public Vector3 Calculate(GameTime gameTime)
        {
            var jitter = _jitter * ((float)gameTime.ElapsedGameTime.TotalSeconds);

            var add = new Vector3((float) ((_random.NextDouble()*2) - 1)*jitter, 0,
                (float) ((_random.NextDouble()*2) - 1)*jitter);
            wanderTarget += add;

            wanderTarget = Vector3.Normalize(wanderTarget)*_radius;

            return Transform.PointToWorldSpace(Agent.Position, Agent.Heading, Vector3.Up, Agent.Side, wanderTarget + new Vector3(_distance, 0, 0)) - Agent.Position; 
        }

        #endregion
    }
}
