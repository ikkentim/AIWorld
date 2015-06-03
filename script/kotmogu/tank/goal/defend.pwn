/*
 * This goal defends a nearby ally.
 *
 */
#include <a_goal>

#include "../common/status"
#include "../common/combat"
#include "../common/movement"

new static
    Float:total_time,
    targetid,
    SB:pursuit,
    hasbeenincombat;

main() { }

public OnEnter()
{
    if(hasbeenincombat)
    {
        Terminate();
        return;
    }

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
    pursuit = AddOffsetPursuit(0.3, targetid, -1.5, 0);
    ToggleMovementBehaviors(true);
}

public OnUpdate(Float:elapsed)
{
    total_time += elapsed;

    if(AttackIfEnemyNearby())
    {
        UpdateStatus("Entering combat mode");
        hasbeenincombat=true;
        return;
    }

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

public OnExit()
{
    RemoveSteeringBehavior(pursuit);
    ToggleMovementBehaviors(false);
}
