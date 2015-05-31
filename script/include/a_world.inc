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
<param name="name">
    An array variable in which to store the name of the graph.
</param>
<param name="len">The length of the name array.</param>
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

/** ************************************************************************ **\
<summary>Gets the position of the entity with the specified id.</summary>
<param name="x">
    A float variable in which to store the x-coordinate in, passed by reference.
</param>
<param name="y">
    A float variable in which to store the y-coordinate in, passed by reference.
</param>
<returns>This function does not return a specific value.</returns>
\** ************************************************************************ **/
native GetEntityPosition(id, &Float:x, &Float:y);

/** ************************************************************************ **\
<summary>
    Finds the id of the nearest world object with the specified model name.
</summary>
<param name="x">The x-coordinate of where to look.</param>
<param name="y">The y-coordinate of where to look.</param>
<param name="model">
    The name of the model for which to look. If none is specified, any model
    passes.
</param>
<returns>The id of the found entity. -1 if none was found.</returns>
\** ************************************************************************ **/
native FindNearestWorldObject(Float:x, Float:y, const model[] = '''');

//TODO add docs
native SetWorldObjectTranslation(id, Float:x, Float:y, Float:z);
native SetWorldObjectScale(id, Float:x, Float:y, Float:z);
native SetWorldObjectRotation(id, Float:x, Float:y, Float:z);
native RemoveWorldObject(id);

/** ************************************************************************ **\
<summary>
    Finds the id of the nearest world object with the specified script name.
</summary>
<param name="x">The x-coordinate of where to look.</param>
<param name="y">The y-coordinate of where to look.</param>
<param name="range">The range in which to look.</param>
<param name="scriptname">The name of the script for which to look.</param>
<returns>The id of the found entity. -1 if none was found.</returns>
\** ************************************************************************ **/
native FindNearestAgent(Float:x, Float:y, Float:range,
    const scriptname[] = '''');

native FindNearestAgentByVar(Float:x, Float:y, Float:range, const key[], value,
    const scriptname[] = '''');
native FindNearestAgentByVarFloat(Float:x, Float:y, Float:range, const key[],
    Float:value, const scriptname[] = '''');
native FindNearestAgentByVarString(Float:x, Float:y, Float:range, const key[],
    value[], const scriptname[] = '''');
native GetNearestAgent(Float:x, Float:y, Float:range, const scriptname[],
    agents[], length = sizeof agents);
native GetNearestAgentByVar(Float:x, Float:y, Float:range, const key[], value,
    const scriptname[], agents[], length = sizeof agents);
native GetNearestAgentByVarFloat(Float:x, Float:y, Float:range, const key[],
    Float:value, const scriptname[], agents[], length = sizeof agents);
native GetNearestAgentByVarString(Float:x, Float:y, Float:range, const key[],
    value[], const scriptname[], agents[], length = sizeof agents);

native SetAgentVar(agentid, const key[], value);
native SetAgentVarFloat(agentid, const key[], Float:value);
native SetAgentVarstring(agentid, const key[], value[]);
native DeleteAgentVar(agentid, const key[]);
native GetAgentVar(agentid, const key[]);
native Float:GetAgentVarFloat(agentid, const key[]);
native GetAgentVarString(agentid, const key[], value[], len);

native SetGVar(const key[], value);
native SetGVarFloat(const key[], Float:value);
native SetGVarstring(const key[], value[]);
native DeleteGVar(const key[]);
native GetGVar(const key[]);
native Float:GetGVarFloat(const key[]);
native GetGVarString(const key[], value[], len);

native CallPublicFunction(const function[], const format[], {Float,_}:...);
native CallRemoteFunction(const function[], const format[], {Float,_}:...);

native Float:GetEntitySize(entityid);

/** ************************************************************************ **\
<summary>
    Sents the specified message to the specified agent and it's goals.
</summary>
<param name="receiverid">
    The id of the agent which should receive this message.
</param>
<param name="message">The message to sent.</param>
<param name="contents">The contents of the message to sent.</param>
<returns>
    True if the message was successfully received; False otherwise.
</returns>
\** ************************************************************************ **/
native SendMessage(receiverid, message, contents=0);

/** ************************************************************************ **\
<summary>
    Sents the specified message to the every agent and their goals.
</summary>
<param name="message">The message to sent.</param>
<param name="contents">The contents of the message to sent.</param>
<returns>The number of recipients.</returns>
\** ************************************************************************ **/
native SendMessageToAll(message, contents=0);
