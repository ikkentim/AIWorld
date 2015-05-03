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
<returns>The number of nodes added to the graph.</returns>
\** ************************************************************************ **/
native FillGraph(const name[], Float:minX, Float:minY, Float:maxX,
    Float:maxY, Float:offset);

/** ************************************************************************ **\
<summary>Gets the number of existing graphs.</summary>
<returns>The number of existing graphs.</returns>
\** ************************************************************************ **/
native GetGraphCount();

/** ************************************************************************ **\
<summary>Gets the name of the graph at the specified index.</summary>
<returns>
    True if successful; False otherwise (e.g. index out of bounds).
</returns>
\** ************************************************************************ **/
native GetGraphName(index, name[], len = sizeof name);

/** ************************************************************************ **\
<summary>Toggles drawing of all graphs.</summary>
<param name="toggle">If true graphs will be drawn.</param>
<returns>This function does not return a specific value.</returns>
\** ************************************************************************ **/
native SetDrawGraphs(toggle);

/** ************************************************************************ **\
<summary>Gets whether graphs are being drawn.</summary>
<returns>If true graphs will be drawn; False otherwise.</returns>
\** ************************************************************************ **/
native GetDrawGraphs();

/** ************************************************************************ **\
<summary>Sets the target position of the camera.</summary>
<param name="x">Sets the x-coordinate of the target position.</param>
<param name="y">Sets the y-coordinate of the target position.</param>
<returns>This function does not return a specific value.</returns>
\** ************************************************************************ **/
native SetTarget(Float:x, Float:y);

/** ************************************************************************ **\
<summary>Sets the target entity of the camera.</summary>
<param name="id">The id of the entity to target.</param>
<returns>This function does not return a specific value.</returns>
\** ************************************************************************ **/
native SetTargetEntity(id);
