/*
 * This goal gets the nearest orb.
 *
 */
#include <a_goal>

#include "../common/status"
#include "../common/combat"
#include "../common/movement"

new static
    bool:isnavigatingtotarget,
    bool:isincombat,
    targetid;

main() { }

bool:IsInRangeOfTarget()
{
    new Float:x,
        Float:y;

    // Check whether the tank is in range of the target.
    GetEntityPosition(targetid, x, y);
    return IsInRangeOfPoint(x, y, FIRE_RANGE);
}

Attack()
{
    // If already moving towards the target or in combat, return from the
    // function.
    if(isnavigatingtotarget || isincombat)
        return;

    // If in range of the target, add the combat goal to the stack. Otherwise,
    // add the path towards the target to the stack.
    if(IsInRangeOfTarget())
    {
        isincombat = true;
        AddSubgoal("kotmogu/tank/goal/combat");
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
    // Find the nearest enemy TODO: with an orb.
    targetid = GetNearestEnemy();

    if(targetid < 0)
    {
        logprintf(COLOR_RED,"ERROR: Could not find enemy to attack.");
        Terminate();
        return;
    }

    // Log a message to the console.
    UpdateStatus("Going to attack");

    Attack();
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

    // If for some reason, the tank is not ready for combat, terminate this
    // goal.
    if(!IsReadyForCombat())
    {
        Terminate();
        return;
    }

    // If the tank is navigating to the target and is in range of the target,
    // stop moving to the target and start attacking the target.
    if(isnavigatingtotarget && IsInRangeOfTarget())
    {
        isnavigatingtotarget = false;

        // Stop following the path and remove it from the stack.
        TerminateSubgoals();
        ClearPathStack();

        // Start attacking.
        Attack();
    }

    // If the target is dead, find a new target. If none were found, terminate
    // this goal.
    if(!GetAgentVar(targetid, "team"))
    {
        targetid = GetNearestEnemy();

        if(targetid < 0)
        {
            Terminate();
            return;
        }
    }
}

public OnExit()
{
    // If this goal has subgoals, terminate them.
    TerminateSubgoals();
    isnavigatingtotarget = false;
    isincombat = false;

    // Drop the calculated path from the stack in case the goal was terminated
    // before reaching the target.
    ClearPathStack();
}
