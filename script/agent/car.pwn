#include <a_agent>

main()
{
    SetModel("models/car");
    SetSize(0.3);
    SetMaxForce(15);
    SetMaxSpeed(1.2);
    SetMass(0.35);
    SetTargetRange(0.75);

    new Float:targetx,
        Float:targety,
        Float:targetnodex,
        Float:targetnodey,
        Float:startx,
        Float:starty;

    targetx = frandom(-5,5);
    targety = frandom(-5,5);

    GetPosition(startx,starty);
    GetClosestNode("road",startx,starty,startx,starty);

    GetClosestNode("road",targetx,targety,targetnodex,targetnodey);

    PushPathNode(targetx,targety);
    PushPath("road",startx,starty,targetnodex,targetnodey);

    AddSteeringBehavior("target", BEHAVIOR_EXPLORE, 0.78, 2.5, 2.5);
    AddSteeringBehavior("avoidance", BEHAVIOR_OBSTACLE_AVOIDANCE, 1.5);
}

forward OnPathEnd();
public OnPathEnd()
{
    new Float:x,
        Float:y;

    GetPosition(x, y);
    logprintf(COLOR_LIME, "[car] OnPathEnd()");
    logprintf(COLOR_LIME, "[car] {%f, %f}", x, y);
}

public OnUpdate()
{
    //logprintf(COLOR_WHITE, "[car] Updated");
}
