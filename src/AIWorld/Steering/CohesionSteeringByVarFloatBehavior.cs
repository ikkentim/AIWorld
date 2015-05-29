using AIWorld.Entities;

namespace AIWorld.Steering
{
    public class CohesionSteeringByVarFloatBehavior : CohesionSteeringBehavior
    {
        public CohesionSteeringByVarFloatBehavior(Agent agent)
            : base(agent)
        {
        }

        [SteeringBehaviorArgument(2)]
        public float Value
        {
            get { return (float)KeyValue; }
            set { KeyValue = value; }
        }
    }
}