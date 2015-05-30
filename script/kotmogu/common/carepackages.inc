#include <math>
#include <a_world>

#define CAREPACKAGE_AMMO_RATIO          (0.5)
#define CAREPACKAGE_HEALTH_RATIO        (0.5)

#define CAREPACKGE_AMMO_MIN_AMOUNT      (10)
#define CAREPACKGE_AMMO_MAX_AMOUNT      (20)
#define CAREPACKGE_HEALTH_MIN_AMOUNT    (20)
#define CAREPACKGE_HEALTH_MAX_AMOUNT    (30)

stock GetNearestPackage(Float:x, Float:y)
{
    return CallRemoteFunction("GetNearestPackage_", "ff", x, y);
}

stock GetCarepackageContents(&ammo, &Float:health)
{
    if(frandom(0, CAREPACKAGE_AMMO_RATIO + CAREPACKAGE_HEALTH_RATIO) <
        CAREPACKAGE_AMMO_RATIO)
        ammo = random2(CAREPACKGE_AMMO_MIN_AMOUNT, CAREPACKGE_AMMO_MAX_AMOUNT);
    else
        health = frandom(CAREPACKGE_HEALTH_MIN_AMOUNT, CAREPACKGE_HEALTH_MAX_AMOUNT);
}

stock RemoveCarepackage(id)
{
    CallPublicFunction("OnPickUpCarepackage", "dd", GetId(), id);
}

forward GetNearestPackage_(Float:x, Float:y);
forward OnPickUpCarepackage(entityid, carepackageid);
