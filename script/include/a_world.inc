#if defined _world_included
    #endinput
#endif
#define _world_included

/** ************************************************************************ **\
<summary>
    Gets the coordinates of the node nearest to the specified coordinates in the
    specified graph.
</summary>
<param name="name">The graph in which to look.</param>
<param name="x">The x-coordinate of where to look.</param>
<param name="y">The x-coordinate of where to look.</param>
<param name="nx">
    A float variable in which to store the x-coordinate in, passed by reference.
</param>
<param name="ny">
    A float variable in which to store the y-coordinate in, passed by reference.
</param>
<returns>This function does not return a specific value.</returns>
\** ************************************************************************ **/
native GetClosestNode(const name[], Float:x, Float:y, &Float:nx, &Float:ny);

/** ************************************************************************ **\
<summary>Creates a new empty graph.</summary>
<param name="name">The name of the graph.</param>
<returns>
    True if succesfully created; False otherwise (e.g. key already in use).
</returns>
\** ************************************************************************ **/
native CreateGraph(const name[]);

/** ************************************************************************ **\
<summary>
    Fills the specified graph with nodes at every specified offset between the
    specified minimum and maximum coordinates.
</summary>
<param name="name">The name of the graph.</param>
<param name="minX">The minimum x-coordinate at which to add nodes.</param>
<param name="minY">The minimum y-coordinate at which to add nodes.</param>
<param name="maxX">The maximum x-coordinate at which to add nodes.</param>
<param name="maxY">The maximum y-coordinate at which to add nodes.</param>
<param name="offset">
    The offset between each horizontal and vertical node.
</param>
<returns>
    True if succesfully filled; False otherwise (e.g. key does not exist).
</returns>
\** ************************************************************************ **/
native FillGraph(const name[], Float:minX, Float:minY, Float:maxX,
    Float:maxY, Float:offset);
