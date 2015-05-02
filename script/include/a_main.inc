#if defined _main_included
    #endinput
#endif
#define _main_included

#define ROTATION_NONE                         0
#define ROTATION_CLOCKWISE_90_DEGREES         3
#define ROTATION_CLOCKWISE_180_DEGREES        2
#define ROTATION_COUNTERCLOCKWISE_90_DEGREES  1

/**--------------------------------------------------------------------------**\
<summary>Adds a game object to the game world.</summary>
<param name="modelname">
The path to the model to draw at the objects position.
</param>
<param name="size">The radius of the model in coordinate points.</param>
<param name="x">The x-coordinate at which to draw the model.</param>
<param name="y">The y-coordinate at which to draw the model.</param>
<param name="angle">The angle at which to draw the model.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within the main script.</remarks>
\**--------------------------------------------------------------------------**/
native AddGameObject(const modelname[], Float:size, Float:x, Float:y,
    Float:angle=0.0);

/**--------------------------------------------------------------------------**\
<summary>Adds a road to the game world.</summary>
<param name="key">The key of the graph in which to put the road nodes.</param>
<param name="pointsxy">
    An array of the points within the road to create. Every pair represents a
    set of x-and y-coordinates.
</param>
<param name="count">The angle at which to draw the model.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within the main script.</remarks>
\**--------------------------------------------------------------------------**/
native AddRoad(const key[], Float:pointsxy[], count = sizeof pointsxy);

/**--------------------------------------------------------------------------**\
<summary>Adds an agent to the game world.</summary>
<param name="scriptname">
    Name of the script within the 'agent' folder to run for the agent.
</param>
<param name="x">The x-coordinate at which to spawn the agent.</param>
<param name="y">The y-coordinate at which to spawn the agent.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within the main script.</remarks>
\**--------------------------------------------------------------------------**/
native AddAgent(const scriptname[], Float:x, Float:y);

/**--------------------------------------------------------------------------**\
<summary>Adds a textured plane to the game world.</summary>
<param name="x">The x-coordinate at which to spawn the agent.</param>
<param name="y">The y-coordinate at which to spawn the agent.</param>
<param name="size">The size of the plane in coordinate points.</param>
<param name="rotation">The rotation of the texture.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within the main script.</remarks>
\**--------------------------------------------------------------------------**/
native AddQuadPlane(Float:x, Float:y, Float:z, Float:size, rotation,
    const texture[]);
