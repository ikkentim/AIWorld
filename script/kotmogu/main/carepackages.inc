#include <math>
#include "common/carepackages"

#define MAX_CAREPACKAGES                    (10)
#define CAREPACKAGE_TIME_TO_LIVE            (120.0)
#define CAREPACKAGE_DROP_TIME               (1.6)
#define CAREPACKAGE_DROP_HEIGHT             (25.0)
#define CAREPACKAGE_DROP_ROTATION_MIN       (0)
#define CAREPACKAGE_DROP_ROTATION_MAX       (2*PI)
#define CAREPACKAGE_SCALE                   (0.075)
#define CAREPACKAGE_MIN_INTERVAL            (10.0)
#define CAREPACKAGE_MAX_INTERVAL            (30.0)
#define CAREPACKAGE_MIN_X                   (-100)
#define CAREPACKAGE_MAX_X                   (100)
#define CAREPACKAGE_MIN_Y                   (-100)
#define CAREPACKAGE_MAX_Y                   (100)

#define CarepackageDescription [
        .id,
        .active,
        .dropping,
        Float:.time,
        Float:.dropRotationEffect,
        Float:.rotation
    ]

new carepackages[MAX_CAREPACKAGES][CarepackageDescription];
new Float:carepackageETA = 5.0;

public OnPickUpCarepackage(entityid, carepackageid)
{
    logprintf(-1, "OnPickUpCarepackage(%d, %d)", entityid, carepackageid);

    for(new i=0;i<MAX_CAREPACKAGES;i++)
        if(carepackages[i].id == carepackageid && carepackages[i].active)
        {
            carepackages[i].active = false;
            logprintf(-1, "REMOVED WORLD OBJECT %d :: %d", carepackageid, RemoveWorldObject(carepackageid));
        }
}

public GetNearestPackage_(Float:x, Float:y)
{
    new Float:dist = 99999999.0;
    new nearest = -1;

    for(new i=0;i<MAX_CAREPACKAGES;i++)
    {
        if(carepackages[i].active)
        {
            new Float:px, Float:py;
            GetEntityPosition(carepackages[i].id, px, py);
            new Float:curDist=fdist(x, y, px, py);
            if(curDist < dist)
            {
                nearest = carepackages[i].id;
                dist = curDist;
            }
        }
    }

    return nearest;
}

CarepackagesUpdate(Float:elapsed)
{
    for(new i=0;i<MAX_CAREPACKAGES;i++)
    {
        if(carepackages[i].active || carepackages[i].dropping)
        {
            carepackages[i].time += elapsed;

            if(carepackages[i].dropping)
            {
                new Float:air = fmax(0, (1 - carepackages[i].time / CAREPACKAGE_DROP_TIME));

                SetWorldObjectTranslation(carepackages[i].id, 0, air * CAREPACKAGE_DROP_HEIGHT * (1 / 0.05/*scale*/), 0);
                SetWorldObjectRotation(carepackages[i].id, 0, carepackages[i].rotation - air * carepackages[i].dropRotationEffect, 0);
                //SetWorldObjectRotation//carepackages[i].dropRotationEffect
                if(carepackages[i].time >= CAREPACKAGE_DROP_TIME)
                {
                    new Float:x, Float:y;
                    GetEntityPosition(carepackages[i].id, x, y);
                    PlaySound("sounds/crate_drop", 0.4, x, y);
                    carepackages[i].dropping = false;
                    carepackages[i].active = true;
                }
            }
        }
    }

    if((carepackageETA-= elapsed) <= 0)
    {
        for(new i=0;i<MAX_CAREPACKAGES;i++)
        {
            if(!carepackages[i].active && !carepackages[i].dropping)
            {
                // Calculate a random LZ
                new Float:x = frandom(CAREPACKAGE_MIN_X, CAREPACKAGE_MAX_X),
                    Float:y = frandom(CAREPACKAGE_MIN_Y, CAREPACKAGE_MAX_Y);

                //TODO: Check space occupied.

                // Fill carepackage data
                carepackages[i].id = AddGameObject("models/ammocrate", 0,
                    x, y, CAREPACKAGE_SCALE, CAREPACKAGE_SCALE, CAREPACKAGE_SCALE);
                carepackages[i].dropping = true;
                carepackages[i].time = 0.0;
                carepackages[i].rotation = frandom(-PI, PI);
                carepackages[i].dropRotationEffect =
                    frandom(CAREPACKAGE_DROP_ROTATION_MIN,
                        CAREPACKAGE_DROP_ROTATION_MAX);

                // Show to user
                PlaySound("sounds/plane_flyby", 0.6, x, y);
                chatprintf(COLOR_RED, "A carepackage was dropped!");

                break;
            }
        }
        carepackageETA = frandom(CAREPACKAGE_MIN_INTERVAL, CAREPACKAGE_MAX_INTERVAL);
        chatprintf(-1, "Next drops in %f secs", carepackageETA);
    }
}
