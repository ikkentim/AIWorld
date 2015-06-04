/*
 * This goal defends a nearby ally.
 *
 */
#include <a_goal>

#include "../common/status"
#include "../common/combat"
#include "../common/movement"

#define RANGE_START_PURSUIT     (15.0)

new static
    Float:totaltime,
    bool:isnavigatingtotarget,
    bool:isincombat,
    position,
    targetid,
    SB:pursuit = INVALID_STEERING_BEHAVIOR;

main() { }

bool:IsInRangeOfTarget()
{
    new Float:x,
        Float:y;

    // Check whether the tank is in range of the target.
    GetEntityPosition(targetid, x, y);
    return IsInRangeOfPoint(x, y, RANGE_START_PURSUIT);
}

MoveToTarget()
{
    // If already approaching the target return from this function.
    if(pursuit != INVALID_STEERING_BEHAVIOR || isnavigatingtotarget)
        return;

    // If the tank is in range of the target, start the pursuit. Otherwise, move
    // to the target.
    if(IsInRangeOfTarget())
    {
        // Add the offset pursuit steering behavior.
        switch(position)
        {
            case 0: pursuit = AddOffsetPursuit(0.3, targetid,  2,  2);
            case 1: pursuit = AddOffsetPursuit(0.3, targetid,  2, -2);
            case 2: pursuit = AddOffsetPursuit(0.3, targetid, -2,  2);
            case 3: pursuit = AddOffsetPursuit(0.3, targetid, -2, -2);
            case 4: pursuit = AddOffsetPursuit(0.3, targetid,  4,  0);
            case 5: pursuit = AddOffsetPursuit(0.3, targetid, -4,  0);
            case 6: pursuit = AddOffsetPursuit(0.3, targetid,  0,  4);
            case 7: pursuit = AddOffsetPursuit(0.3, targetid,  0, -4);
        }
    }
    else
    {
        new Float:x,
            Float:y,
            Float:tx,
            Float:ty;

        // Push the path towards the target to the stack and follow this path.
        isnavigatingtotarget = true;

        GetPosition(x, y);
        GetEntityPosition(targetid, tx, ty);

        GetClosestNode("ground", x, y, x, y);
        GetClosestNode("ground", tx, ty, tx, ty);
        PushPath("ground", x, y, tx, ty);

        AddSubgoal("common/followpath");
    }
}
public OnEnter()
{
    new Float:x,
        Float:y;

    // After pausing this task, restart the timer.
    totaltime = 0.0;

    // Find the nearest teammate with an orb.
    GetPosition(x, y);
    targetid = FindNearestAgentByVar(x, y, 1000, "team", GetVar("team"), "kotmogu/orb");

    if(targetid < 0)
    {
        logprintf(COLOR_RED,"ERROR: Tried to defend but there is no orb holder.");
        Terminate();
        return;
    }

    // Calculate the position near the target.
    new positions = GetAgentVar(targetid, "positions");
    for(new i = 0 ; i < 8 ; i++)
        if(!(positions & (1 << i)))
        {
            position = i;
            positions ^= 1 << position;
            break;
        }
    logprintf(-1, "Defending on position %d", position);
    SetAgentVar(targetid, "positions", positions);

    // Log a message to the console.
    UpdateStatus("Going to defend my team!");

    // Start moving to the target.
    MoveToTarget();
}

public OnUpdate(Float:elapsed)
{
    // Check whether there is an enemy nearby and this tank is ready for combat.
    // If there is, stop path following and pursuit and start attacking.
    if(!isincombat && GetEnemyInFireRange() >= 0 && IsReadyForCombat())
    {
        // If the tank is pursuing it's target, stop the pursuit.
        if(pursuit != INVALID_STEERING_BEHAVIOR)
        {
            RemoveSteeringBehavior(pursuit);
            pursuit = INVALID_STEERING_BEHAVIOR;
        }

        // If this goal has subgoals, terminate them.
        TerminateSubgoals();
        ClearPathStack();
        isnavigatingtotarget = false;

        // Start combat mode.
        logprintf(-1, "defender entered combat mode.");
        isincombat = true;
        AddSubgoal("kotmogu/tank/goal/combat");
        return;
    }

    // If for some reason, the tank is not ready for combat, terminate this
    // goal.
    if(!IsReadyForCombat())
    {
        Terminate();
        return;
    }

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

    // Ensure the tank does not spent more than two minutes in this task.
    if((totaltime += elapsed) > 120)
    {
        Terminate();
        return;
    }

    // If the target tank has died, terminate this goal.
    if(GetAgentVar(targetid, "team") != GetTeam())
    {
        Terminate();
        return;
    }

    // If the tank is navigating to the target, but the path stack is empty,
    // attempt to move to the target again.
    if(isnavigatingtotarget && !GetSubgoalCount())
    {
        isnavigatingtotarget = false;
        MoveToTarget();
    }

    // If the tank is navigating to the target and is in range of the target,
    // stop moving to the target and start pursuing the target.
    if(isnavigatingtotarget && IsInRangeOfTarget())
    {
        // Stop following the path and remove it from the stack.
        TerminateSubgoals();
        ClearPathStack();

        // Start pursuing the target.
        MoveToTarget();
    }
}

public OnExit()
{
    // Unset the position near the target.
    new positions = GetAgentVar(targetid, "positions");
    positions ^= 1 << position;
    SetAgentVar(targetid, "positions", positions);

    // If the tank is pursuing it's target, stop the pursuit.
    if(pursuit != INVALID_STEERING_BEHAVIOR)
    {
        RemoveSteeringBehavior(pursuit);
        pursuit = INVALID_STEERING_BEHAVIOR;
    }

    // If this goal has subgoals, terminate them.
    TerminateSubgoals();
    isnavigatingtotarget = false;
    isincombat = false;

    // Drop the calculated path from the stack in case the goal was terminated
    // before reaching the target.
    ClearPathStack();
}
