#if defined _main_included
    #endinput
#endif
#define _main_included

#define ROTATION_NONE                         0
#define ROTATION_CLOCKWISE_90_DEGREES         3
#define ROTATION_CLOCKWISE_180_DEGREES        2
#define ROTATION_COUNTERCLOCKWISE_90_DEGREES  1

/**
* <summary>
* Adds a game object to the game world.
* </summary>
*
* <param name="modelname">
* The path to the model to draw at the objects position.
* </param>
* <param name="size">
* The radius of the model in coordinate points.
* </param>
* <param name="x">
* The x-coordinate at which to draw the model.
* </param>
* <param name="y">
* The x-coordinate at which to draw the model.
* </param>
* <param name="angle">
* The angle at which to draw the model.
* </param>
*
* <returns>
* The day of the week, where 0 is Saturday and 6 is Friday.
* </returns>
*
* <remarks>
* This function does not return a specific value.
* </remarks>
*/

native AddGameObject(const modelname[], Float:size, Float:x, Float:y, Float:angle=0.0);
native AddRoad(Float:pointsxy[], count = sizeof pointsxy);
native AddAgent(const scriptname[], Float:x, Float:y);
native AddQuadPlane(Float:x, Float:y, Float:z, Float:size, rotation, const texture[]);
