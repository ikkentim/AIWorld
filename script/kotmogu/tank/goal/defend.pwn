/*
 * This goal defends a nearby ally.
 *
 */
#include <a_goal>
#include "../common/status"

new static
    Float:total_time,
    targetid;

main() { }

public OnEnter()
{
    new Float:x, Float:y;
    GetPosition(x, y);
    targetid = FindNearestAgentByVar(x, y, 1000, "team", GetVar("team"), "kotmogu/orb");

    if(targetid < 0)
    {
        logprintf(COLOR_RED,"ERROR: Tried to defend but there is holder.");
        Terminate();
        return;
    }

    UpdateStatus("Going to defend my team!");
    AddOffsetPursuit(0.3, targetid, -1.5, 0);
}

public OnUpdate(Float:elapsed)
{
    total_time += elapsed;

    // TODO: Tail teammate
    // TODO: Check for death

    if(total_time > 60)
    {
        Terminate();
        return;
    }

    if(GetAgentVar(targetid, "team") != GetVar("team"))
    {
        Terminate();
        return;
    }
}
