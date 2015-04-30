#if defined _world_included
    #endinput
#endif
#define _world_included

/**
* <summary>
* Gets the coordinates of the node nearest to the specified coordinates.
* </summary>
*
* <param name="x">
* The x-coordinate of where to look.
* </param>
* <param name="y">
* The x-coordinate of where to look.
* </param>
* <param name="nx">
* A float variable in which to store the x-coordinate in, passed by reference.
* </param>
* <param name="ny">
* A float variable in which to store the y-coordinate in, passed by reference.
* </param>
*
* <returns>
* This function does not return a specific value.
* </returns>
*/
native GetClosestNode(Float:x, Float:y, &Float:nx, &Float:ny);
