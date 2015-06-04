/*
 * This goal gets the nearest orb.
 *
 */
#include <a_goal>

#include "../common/status"
#include "../common/combat"
#include "../common/movement"

new static
    targetid;

main() { }

public OnEnter()
{
    new Float:x,
        Float:y,
        Float:tx,
        Float:ty;

    // Find the nearest orb based on the position of the tank.
    GetPosition(x, y);
    targetid = FindNearestAgentByVar(x, y, 1000, "team", 0, "kotmogu/orb");
    if(targetid < 0)
    {
        logprintf(COLOR_RED,"ERROR: Tried to get orb but there is no orb.");
        Terminate();
        return;
    }
    GetEntityPosition(targetid, tx, ty);

    // Log a message to the console.
    UpdateStatus("Going to orb");

    // Calculate the path to the o.
    PushPathNode(tx, ty);
    GetClosestNode("ground", x, y, x, y);
    GetClosestNode("ground", tx, ty, tx, ty);
    PushPath("ground", x, y, tx, ty);

    // Add a subgoal to follow the path in the path stack.
    AddSubgoal("common/followpath");
}

public OnUpdate(Float:elapsed)
{
    // TODO: Check for enemies, stop following the path and combat the enemy.

    // If the tank has an orb, the goal has been reached.
    if(GetVar("orb") >= 0)
    {
        UpdateStatus("Got ORB!");

        Terminate();
        return;
    }

    // If the orb is owned by a team, it has been picked up by another tank.
    if(GetAgentVar(targetid, "team") != 0)
    {
        UpdateStatus("Failed to get ORB.");

        Terminate();
        return;
    }
}

public OnExit()
{
    // Drop the calculated path from the stack in case the tank has reached the
    // orb before is has reached the target of the path or if the tank was
    // beaten to the orb.
    ClearPathStack();
}
