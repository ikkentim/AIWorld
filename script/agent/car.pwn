#include <a_agent>

/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of this agent.</summary>
\**--------------------------------------------------------------------------**/
main()
{
    // Set agent properties
    SetModel("models/car");
    SetSize(0.3);
    SetMaxForce(15);
    SetMaxSpeed(1.2);
    SetMass(0.35);
    SetTargetRange(0.75);

    // Generate a random target between (-5,-5) and (5,5).
    new Float:targetx,
        Float:targety,
        Float:targetnodex,
        Float:targetnodey,
        Float:startx,
        Float:starty;

    targetx = frandom(-5, 5);
    targety = frandom(-5, 5);

    // Find nodes closest to my position and my target's.
    GetPosition(startx,starty);
    GetClosestNode("road", startx, starty, startx, starty);
    GetClosestNode("road", targetx, targety, targetnodex, targetnodey);

    // Push a path towards my target by road to the stack.
    PushPathNode(targetx,targety);
    PushPath("road",startx,starty,targetnodex,targetnodey);

    // The road generated above is currently not used...

    // Add two steering behaviours to the queue.
    AddSteeringBehavior("target", BEHAVIOR_EXPLORE, 0.78, 2.5, 2.5);
    AddSteeringBehavior("avoidance", BEHAVIOR_OBSTACLE_AVOIDANCE, 1.5);
}

// Currently no update logic.
/*public OnUpdate()
{
    logprintf(COLOR_WHITE, "[car] Updated");
}*/
