/*
 * This goal gets the nearest carepackage.
 *
 */
#include <a_goal>
#include <math>
#include <a_sound>

#include "../../common/carepackages"
#include "../common/status"
#include "../common/combat"
#include "../common/movement"

#define CAREPACKAGE_RANGE   (2.0)

new static
    targetid,
    Float:targetx,
    Float:targety;

main() { }

public OnEnter()
{
    new Float:x,
        Float:y,
        Float:tx,
        Float:ty;

    // Find the nearest carepackage based on the position of the tank.
    GetPosition(x, y);
    targetid = GetNearestPackage(x, y);
    if(targetid < 0)
    {
        logprintf(COLOR_RED,"ERROR: Tried to get carepackage but there is no \
        carepackage.");
        Terminate();
        return;
    }
    GetEntityPosition(targetid, targetx, targety);

    // Log a message to the console.
    UpdateStatus("Going to carepackage");

    // Calculate the path to the carepackage.
    PushPathNode(targetx, targety);
    GetClosestNode("ground", x, y, x, y);
    GetClosestNode("ground", targetx, targety, tx, ty);
    PushPath("ground", x, y, tx, ty);

    // Add a subgoal to follow the path in the path stack.
    AddSubgoal("common/followpath");
}

public OnUpdate(Float:elapsed)
{
    // TODO: Check for enemies, stop following the path and combat the enemy.

    // If the tank is within range of the carepackage.
    if(IsInRangeOfPoint(targetx, targety, CAREPACKAGE_RANGE))
    {
        new ammo,
            Float:x,
            Float:y,
            Float:health;

        GetPosition(x, y);

        // Generate random carepackage contents and add it to the inventory of
        // this tank.
        GetCarepackageContents(ammo, health);

        if(ammo > 0)
        {
            chatprintf(COLOR_RED, "Picked up %d ammo", ammo);
            PlaySound("sounds/ammo", 1.0, x, y);
            SetVar("ammo", min(GetVar("ammo") + ammo, 40));
        }
        if(health > 0)
        {
            chatprintf(COLOR_RED, "Picked up %f health", health);
            PlaySound("sounds/repair",  1.0, x, y);
            SetVarFloat("health", fmin(GetVarFloat("health") + health, 100));
        }

        // Remove the carepackage from the game and terminate this goal.
        RemoveCarepackage(targetid);
        Terminate();
    }
}

public OnPickUpCarepackage(entityid, carepackageid)
{
    // If someone else has picked up the carepackage, terminate the goal.
    if(entityid != GetId())
    {
        logprintf(-1, "Someone was beaten to a carepackage.");
        Terminate();
    }
}

public OnExit()
{
    // Drop the calculated path from the stack in case the tank has reached the
    // carepackage before is has reached the target of the path or if the tank
    // was beaten to the orb.
    ClearPathStack();
}
