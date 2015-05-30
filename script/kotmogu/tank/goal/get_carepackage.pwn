#include <a_goal>
#include <math>
#include <a_sound>

#include "../../common/carepackages"
#include "../common/status"

#define CAREPACKAGE_RANGE   (2.0)

new SB:seek,
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
}

public OnUpdate(Float:elapsed)
{
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
            PlaySound("sounds/ammo", 0.5, x, y);
        }
        if(health > 0)
        {
            chatprintf(COLOR_RED, "Picked up %f health", health);
            PlaySound("sounds/repair",  0.5, x, y);
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

    return 1;
}

public OnExit()
{
    RemoveSteeringBehavior(seek);
}
