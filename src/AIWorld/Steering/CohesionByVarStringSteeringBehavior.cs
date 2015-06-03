using AIWorld.Entities;

namespace AIWorld.Steering
{
    public class CohesionByVarStringSteeringBehavior : CohesionSteeringBehavior
    {
        public CohesionByVarStringSteeringBehavior(Agent agent)
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