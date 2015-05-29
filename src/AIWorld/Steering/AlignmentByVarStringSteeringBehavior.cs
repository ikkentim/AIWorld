using AIWorld.Entities;

namespace AIWorld.Steering
{
    public class AlignmentByVarStringSteeringBehavior : AlignmentSteeringBehavior
    {
        public AlignmentByVarStringSteeringBehavior(Agent agent)
            : base(agent)
        {
        }

        [SteeringBehaviorArgument(2)]
        public string Value
        {
            get { return (string)KeyValue; }
            set { KeyValue = value; }
        }
    }
}