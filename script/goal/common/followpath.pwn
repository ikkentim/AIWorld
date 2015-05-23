#include <a_goal>

public weight = 0;

/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of the goal.</summary>
\**--------------------------------------------------------------------------**/
main()
{
    //
}

public OnEnter()
{
    //
}

public OnUpdate(Float:elapsed)
{
    new Float:nextx, Float:nexty;

    if(PeekPathNode(nextx, nexty))
    {
        if(IsInTargetRangeOfPoint(nextx, nexty))
        {
            // We're already at this node. Pop it from the stack.
            PopPathNode(nextx, nexty);
        }
        else
        {
            if(GetPathStackSize() > 1)
                AddSubgoal("common/seek", "x,y,weight", nextx, nexty,
                weight);
            else
                AddSubgoal("common/arrive", "x,y,weight", nextx, nexty,
                weight);
        }
    }
    else
    {
        // No more points in path
        Terminate();
    }
}

public OnExit()
{
    //
}

public OnIncomingMessage(message, contents)
{
    //
}
