#include <a_goal>

new Float:targetX;
new Float:targetY;

/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of the goal.</summary>
\**--------------------------------------------------------------------------**/
main()
{

}

public OnEnter()
{
    logprintf(COLOR_WHITE, "Explore.OnEnter");

    AddSteeringBehavior("exploring", BEHAVIOR_EXPLORE, 0.78, 3.0, 3.0);

    GetPosition(targetX, targetY);
    targetX += 3.0;
    targetY += 3.0;
}

public OnUpdate()
{
    //logprintf(COLOR_WHITE, "Explore.Update %f", GetDistanceToPoint(targetX, targetY));

    new Float:x, Float:y;
    GetPosition(x, y);
    if(fabs(x - targetX) < 0.5)
    {
        chatprintf(COLOR_WHITE, "CAR: Done exploring!");
        Terminate();
    }
}

public OnExit()
{
    logprintf(COLOR_WHITE, "Explore.OnExit");

    RemoveSteeringBehavior("exploring");
}

public OnIncomingMessage(message, contents)
{
    //
}