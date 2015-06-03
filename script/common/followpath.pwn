#include <a_goal>

public weight = 0;      // The weight of the steering behavior.
public arrive = true;   // If true, arrive at the last node; otherwise seek.
main() { }

public OnUpdate(Float:elapsed)
{
    new Float:nextx, Float:nexty;

    // Check whether there is any point left in the path stack and store its
    // coordinates in nextx and nexty. If there are no more points on the
    // path stack, terminate this goal.
    if(PeekPathNode(nextx, nexty))
    {
        if(IsInTargetRangeOfPoint(nextx, nexty))
        {
            // We're already at this node. Pop it from the stack.
            PopPathNode(nextx, nexty);
        }
        else
        {
            // Add the steering behavior goal to the subgoals stack.
            if(GetPathStackSize() > 1 || !arrive)
                AddSubgoal("common/seek", "x,y,weight", nextx, nexty,
                weight);
            else
                AddSubgoal("common/arrive", "x,y,weight", nextx, nexty,
                weight);
        }
    }
    else
    {
        Terminate();
    }
}
