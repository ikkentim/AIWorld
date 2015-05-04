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
<param name="angle">The angle at which to draw the model.</param>
<returns>The id of the added game object.</returns>
<remarks>This function can only be used within the main script.</remarks>
\** ************************************************************************ **/
native AddGameObject(const modelname[], Float:size, Float:x, Float:y,
    Float:angle=0.0);

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
