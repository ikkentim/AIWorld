#include <a_goal>

public x = 0;       // The target x-coordinate.
public y = 0;       // The target y-coordinate.
public weight = 0;  // The weight of the steering behavior.

new SB:seek;

main() { }

public OnEnter()
{
    // Add the steering behaviour to the pool.
    seek = AddSeek(weight == 0 ? 0.78 : Float:weight, Float:x, Float:y);
}

public OnUpdate(Float:elapsed)
{
    // If in the range of the target, terminate the goal.
    if(IsInTargetRangeOfPoint(Float:x, Float:y))
    {
        Terminate();
    }
}

public OnExit()
{
    // Remove the steering behavior from the pool.
    RemoveSteeringBehavior(seek);
}
