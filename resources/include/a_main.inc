#if defined _main_included
    #endinput
#endif
#define _main_included

#define ROTATION_NONE                         0
#define ROTATION_CLOCKWISE_90_DEGREES         3
#define ROTATION_CLOCKWISE_180_DEGREES        2
#define ROTATION_COUNTERCLOCKWISE_90_DEGREES  1

native AddGameObject(const modelname[], Float:size, Float:x, Float:y, Float:angle=0.0);
native AddRoad(Float:pointsxy[], count = sizeof pointsxy);
native AddAgent(const scriptname[], Float:x, Float:y);
native AddQuadPlane(Float:x, Float:y, Float:z, Float:size, rotation, const texture[]);
