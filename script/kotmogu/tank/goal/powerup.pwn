/*
 * This goal tries to get to a more powerful area.
 *
 */
#include <a_goal>
#include "../../common/area"
#include "../common/status"
#include "../common/movement"

new static
    targetPower;

main()
{
    new Float:x,
        Float:y;

    // Calculate the target power based on the position of the tank when this
    // goal was created.
    GetPosition(x, y);
    targetPower = GetAreaPower(x, y) + 1;

    // If the tank was already in the most powerful area, terminate this goal.
    if(targetPower > 4)
    {
        logprintf(COLOR_RED, "ERROR: 'powerup' goal was added, but tank is \
            already in the most powerful area!");
        Terminate();
    }
}

public OnEnter()
{
    new Float:x,
        Float:y,
        Float:tx,
        Float:ty;
    GetPosition(x, y);

    // Find the nearest position for the target area. If no suitable area was
    // found, terminate this goal.
    if(!GetNearestAreaForPower(x, y, targetPower, tx, ty))
    {
        logprintf(COLOR_RED, "ERROR: Could not find more powerful area!");
        Terminate();
        return;
    }

    // Log a message to the console.
    UpdateStatus("Powering up!");
    logprintf(-1, "Powering up to %f, %f %d", tx, ty, targetPower);

    // Calculate the path to the area.
    GetClosestNode("ground", x, y, x, y);
    GetClosestNode("ground", tx, ty, tx, ty);
    PushPath("ground", x, y, tx, ty);

    AddSubgoal("common/followpath");
}

public OnUpdate(Float:elapsed)
{
    // TODO: Check for enemies, stop following the path and combat the enemy.

    // Check whether the tank has reached the target area.
    new Float:x, Float:y;
    GetPosition(x, y);

    if(targetPower <= GetAreaPower(x, y))
    {
        Terminate();
        return;
    }
}

public OnExit()
{
    // Drop the calculated path from the stack in case the tank has reached the
    // target area before is has reached the target of the path.
    ClearPathStack();
}
