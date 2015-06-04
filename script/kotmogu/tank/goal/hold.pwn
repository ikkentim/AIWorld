/*
 * This goal holds stance.
 *
 */
#include <a_goal>

#include "../../common/area"
#include "../common/movement"
#include "../common/combat"
#include "../common/status"

new static
    Float:totaltime,
    bool:isincombat,
    power,
    SB:wander = INVALID_STEERING_BEHAVIOR;


main() { }

public OnEnter()
{
    // After pausing this task, restart the timer.
    totaltime = 0.0;

    // Log a message to the console.
    UpdateStatus("Holding stance.");

    // Add steering behavior.
    new Float:x,
        Float:y;
    GetPosition(x, y);
    power = GetAreaPower(x, y);
    wander = AddWander(0.2, 35, 1, 3);
    ToggleMovementBehaviors(false);
}

public OnUpdate(Float:elapsed)
{
    // If the tank is in combat, wait for it to exit combat mode.
    if(isincombat)
    {
        if(!GetSubgoalCount())
        {
            isincombat = false;

            // After combat mode, terminate this goal to recalculate it's goals.
            Terminate();
        }
        return;
    }

    // Ensure the tank does not spent more than one minute in this task.
    if((totaltime += elapsed) > 60)
    {
        Terminate();
        return;
    }

    // If there is a nearby enemy, attack it.
    if(AttackIfEnemyNearby())
    {
        RemoveSteeringBehavior(wander);
        wander = INVALID_STEERING_BEHAVIOR;
        isincombat = true;
        return;
    }

    // Check whether the tank hasn't lost area power.
    new Float:x,
        Float:y;
    GetPosition(x, y);
    if(power > GetAreaPower(x, y))
    {
        Terminate();
    }
}

public OnExit()
{
    if(wander != INVALID_STEERING_BEHAVIOR)
        RemoveSteeringBehavior(wander);
    ToggleMovementBehaviors(true);

    // If this goal has subgoals, terminate them.
    TerminateSubgoals();
    isincombat = false;
}
