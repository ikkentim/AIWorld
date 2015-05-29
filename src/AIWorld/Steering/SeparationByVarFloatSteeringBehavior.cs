using AIWorld.Entities;

namespace AIWorld.Steering
{
    public class SeparationByVarFloatSteeringBehavior : SeparationSteeringBehavior
    {
        public SeparationByVarFloatSteeringBehavior(Agent agent)
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