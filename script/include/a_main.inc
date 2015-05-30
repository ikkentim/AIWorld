#if defined _main_included
    #endinput
#endif
#define _main_included

#define ROTATION_NONE                         0
#define ROTATION_CLOCKWISE_90_DEGREES         3
#define ROTATION_CLOCKWISE_180_DEGREES        2
#define ROTATION_COUNTERCLOCKWISE_90_DEGREES  1

/** ************************************************************************ **\
<summary>Adds a game object to the game world.</summary>
<param name="modelname">
The path to the model to draw at the objects position.
</param>
<param name="size">The radius of the model in coordinate points.</param>
<param name="x">The x-coordinate at which to draw the model.</param>
<param name="y">The y-coordinate at which to draw the model.</param>
<param name="sx">The scale of the x-axis.</param>
<param name="sy">The scale of the y-axis.</param>
<param name="sz">The scale of the z-axis.</param>
<param name="rx">The rotation on the x-axis.</param>
<param name="ry">The rotation on the y-axis.</param>
<param name="rz">The rotation on the z-axis.</param>
<param name="tx">
    The translation on the x-axis before scaling and rotating.
</param>
<param name="ty">
    The translation on the y-axis before scaling and rotating.
</param>
<param name="tz">
    The translation on the z-axis before scaling and rotating.
</param>
<param name="meshes">
    The names of the visible meshes, seperated by a ','. If no meshes are
    specified all meshes will be displayed.
</param>
<returns>The id of the added game object.</returns>
<remarks>This function can only be used within the main script.</remarks>
\** ************************************************************************ **/
native AddGameObject(const modelname[], Float:size, Float:x, Float:y,
    Float:sx=1.0, Float:sy=1.0, Float:sz=1.0, Float:rx=0.0, Float:ry=0.0,
    Float:rz=0.0, Float:tx=0.0, Float:ty=0.0, Float:tz=0.0,
    const meshes[]='''');

//TODO: add docs
native RemoveEntity(id);

/** ************************************************************************ **\
<summary>Adds a road to the game world.</summary>
<param name="key">The key of the graph in which to put the road nodes.</param>
<param name="pointsxy">
    An array of the points within the road to create. Every pair represents a
    set of x-and y-coordinates.
</param>
<param name="count">The angle at which to draw the model.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within the main script.</remarks>
\** ************************************************************************ **/
native AddRoad(const key[], Float:pointsxy[], count = sizeof pointsxy);

/** ************************************************************************ **\
<summary>Adds an agent to the game world.</summary>
<param name="scriptname">
    Name of the script within the 'agent' folder to run for the agent.
</param>
<param name="x">The x-coordinate at which to spawn the agent.</param>
<param name="y">The y-coordinate at which to spawn the agent.</param>
<param name="y">The y-coordinate at which to spawn the agent.</param>
<param name="format">
    The parameter names and types to be set after creation.
</param>
<returns>The id of the added agent.</returns>
<remarks>This function can only be used within the main script.</remarks>
\** ************************************************************************ **/
native AddAgent(const scriptname[], Float:x, Float:y, const format[]='''', {Float,_}:...);

/** ************************************************************************ **\
<summary>Adds a textured plane to the game world.</summary>
<param name="x">The x-coordinate at which to spawn the agent.</param>
<param name="y">The y-coordinate at which to spawn the agent.</param>
<param name="size">The size of the plane in coordinate points.</param>
<param name="rotation">The rotation of the texture.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within the main script.</remarks>
\** ************************************************************************ **/
native AddQuadPlane(Float:x, Float:y, Float:z, Float:size, rotation,
    const texture[]);

/** ************************************************************************ **\
<summary>Sets the background color of the game.</summary>
<param name="color">The background color of the game.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within the main script.</remarks>
\** ************************************************************************ **/
native SetBackgroundColor(color);

/** ************************************************************************ **\
<summary>
    Plays the specified ambience sound with te specified volume, pitch and pan.
</summary>
<param name="sound">The path to the sound file.</param>
<param name="isLooped">
    If true, the sound will start over once it has ended.
</param>>
<param name="volume">The volume of the ambience sound.</param>
<param name="pitch">The pitch of the ambience sound.</param>
<param name="pan">The pan of the ambience sound.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within the main script.</remarks>
\** ************************************************************************ **/
native PlayAmbience(const sound[], isLooped=false, Float:volume=1.0,
    Float:pitch=0.0, Float:pan=0.0);

/** ************************************************************************ **\
<summary>
    Stops all ambience sounds.
</summary>
<returns>The number of stopped sounds.</returns>
<remarks>This function can only be used within the main script.</remarks>
\** ************************************************************************ **/
native StopAmbience();

/** ************************************************************************ **\
<summary>
    Changes the simulation to the script to the simulated path.
</summary>
<param name="newMode">The new script path.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within the main script.</remarks>
\** ************************************************************************ **/
native ChangeMode(const newMode[]);

/** ************************************************************************ **\
<summary>This callback is called when the keyboard state changes.</summary>
<param name="newKeys">
    An array of all the keys pressed after the change of state.
</param>
<param name="oldKeys">
    An array of all the keys pressed before the change of state.
</param>
<returns>This callback does not handle return values.</returns>
\** ************************************************************************ **/
forward OnKeyStateChanged(newKeys[], oldKeys[]);

/** ************************************************************************ **\
<summary>This callback is called when the user clicks somewhere.</summary>
<param name="button">The pressed button.</param>
<param name="x">The x-coordinate of the clicked position.</param>
<param name="y">The y-coordinate of the clicked position.</param>
<returns>This callback does not handle return values.</returns>
\** ************************************************************************ **/
forward OnMouseClick(button, Float:x, Float:y);

//TODO docs
forward OnUpdate(Float:elapsed);
