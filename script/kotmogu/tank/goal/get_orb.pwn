/*
 * This goal gets the nearest orb.
 *
 */
#include <a_goal>

#include "../common/status"
#include "../common/combat"

new static
    SB:seek,
    targetid;

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
    ToggleMovementBehaviors(true);
}

public OnUpdate(Float:elapsed)
{
    if(AttackIfEnemyNearby()) return;

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
    ToggleMovementBehaviors(false);
}
