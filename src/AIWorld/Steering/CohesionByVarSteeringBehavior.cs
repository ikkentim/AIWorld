using AIWorld.Entities;

namespace AIWorld.Steering
{
    public class CohesionByVarSteeringBehavior : CohesionSteeringBehavior
    {
        public CohesionByVarSteeringBehavior(Agent agent)
            : base(agent)
        {
        }

        [SteeringBehaviorArgument(2)]
        public int Value
        {
            get { return (int)KeyValue; }
            set { KeyValue = value; }
        }
    }
}