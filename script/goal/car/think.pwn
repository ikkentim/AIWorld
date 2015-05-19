#include <a_goal>
#include <time>

/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of the goal.</summary>
\**--------------------------------------------------------------------------**/
main()
{
    //
}

public OnEnter()
{
    logprintf(COLOR_WHITE, "Think.OnEnter");

    //
}

public OnUpdate(Float:elapsed)
{
    logprintf(COLOR_WHITE, "Think.OnUpdate");

    //TODO: Add some state checking...
    //chatprintf(COLOR_WHITE, "CAR: I think I should explore this area...");
    //AddSubgoal("car/explore");

    // Calculate a target position
    new Float:tx = frandom(-10,10);
    new Float:ty = frandom(-10,10);
    new Float:x, Float:y, Float:nodes[4];

    // Get the current position
    GetPosition(x,y);

    // Get nodes closest to current position and target position
    GetClosestNode("road", x, y, nodes[0], nodes[1]);
    GetClosestNode("road", tx, ty, nodes[2], nodes[3]);

    // Calcualte a path in the 'ground' graph
    PushPath("road", nodes[0], nodes[1], nodes[2], nodes[3]);

    // Add path following to goals list
    chatprintf(COLOR_WHITE, "I've found myself a new target.");
    AddSubgoal("common/followpath", "weight", 0.7);
}

public OnExit()
{
    logprintf(COLOR_WHITE, "Think.OnExit");

    //
}

public OnIncomingMessage(message, contents)
{
    //
}
