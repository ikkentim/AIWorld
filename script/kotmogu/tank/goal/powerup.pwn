/*
 * This goal tries to get to a more powerful area.
 *
 */
#include <a_goal>
#include "../../common/area"
#include "../common/status"

new static
    SB:seek,
    targetPower;

main() { }

public OnEnter()
{
    new Float:x, Float:y;
    GetPosition(x, y);

    targetPower = GetAreaPower(x, y) + 1;

    if(!GetNearestAreaForPower(x, y, targetPower, x, y))
    {
        logprintf(COLOR_RED, "ERROR: Couldnt find powerup area");
        Terminate();
        return;
    }

    UpdateStatus("Powering up!");
    logprintf(-1, "Powering up to %f, %f %d", x, y, targetPower);
    seek = AddSeek(0.5, x, y);
    ToggleMovementBehaviors(true);
}

public OnUpdate(Float:elapsed)
{
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
    RemoveSteeringBehavior(seek);
    ToggleMovementBehaviors(false);
}
