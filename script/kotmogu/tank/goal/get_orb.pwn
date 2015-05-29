#include <a_goal>

#include "../common/status"

new SB:seek;
new targetid;

main() { }

public OnEnter()
{
    new Float:x, Float:y;
    GetPosition(x, y);
    targetid = FindNearestAgentByVar(x, y, 1000, "team", 0, "kotmogu/orb");
    if(targetid < 0)
    {
        logprintf(COLOR_RED,"ERROR: Tried to get orb but there is no orb.");
        Terminate();
        return;
    }

    UpdateStatus("Going to orb");

    GetEntityPosition(targetid, x, y);
    seek = AddSeek(0.5, x, y);
}

public OnUpdate(Float:elapsed)
{
    if(GetAgentVar(targetid, "team") != 0)
    {
        if(GetVar("orb") >= 0)
        {
            UpdateStatus("Got ORB!");
        }
        else
        {
            UpdateStatus("Failed to get ORB.");
        }
        Terminate();
        return;
    }
}

public OnExit()
{
    RemoveSteeringBehavior(seek);
}
