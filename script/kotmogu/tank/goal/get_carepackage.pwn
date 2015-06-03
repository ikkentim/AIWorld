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
    SB:seek,
    targetid,
    Float:targetx,
    Float:targety;

main() { }

public OnEnter()
{
    new Float:x, Float:y;
    GetPosition(x, y);

    targetid = GetNearestPackage(x, y);
    if(targetid < 0)
    {
        logprintf(COLOR_RED,"ERROR: Tried to get carepackage but there is no carepackage.");
        Terminate();
        return;
    }

    UpdateStatus("Going to carepackage");

    GetEntityPosition(targetid, targetx, targety);
    seek = AddSeek(0.5, targetx, targety);
    ToggleMovementBehaviors(true);
}

public OnUpdate(Float:elapsed)
{
    if(GetSubgoalCount()) return;

    if(AttackIfEnemyNearby()) return;

    new Float:x, Float:y;
    GetPosition(x, y);

    if(fdist(x, y, targetx, targety) <= CAREPACKAGE_RANGE)
    {
        new ammo,
            Float:health;

        // Generate random carepackage contents
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

        RemoveCarepackage(targetid);
        Terminate();
    }
}

public OnPickUpCarepackage(entityid, carepackageid)
{
    if(entityid != GetId())
    {
        logprintf(-1, "Someone was beaten to a carepackage");
        Terminate();
    }
}

public OnExit()
{
    RemoveSteeringBehavior(seek);
    ToggleMovementBehaviors(false);
}
