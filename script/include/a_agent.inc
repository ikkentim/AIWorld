#if defined _agent_included
    #endinput
#endif
#define _agent_included

#define BEHAVIOR_SEEK                   1
#define BEHAVIOR_ARRIVE                 2
#define BEHAVIOR_OBSTACLE_AVOIDANCE     3
#define BEHAVIOR_EXPLORE                4

/** ************************************************************************ **\
<summary>Gets the id of this agent.</summary>
<returns>The id of this agent.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native GetId();

/** ************************************************************************ **\
<summary>Gets the position of this agent.</summary>
<param name="x">
    A float variable in which to store the x-coordinate in, passed by reference.
</param>
<param name="y">
    A float variable in which to store the y-coordinate in, passed by reference.
</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native GetPosition(&Float:x, &Float:y);

/** ************************************************************************ **\
<summary>Gets the direction this agent is facing.</summary>
<param name="x">
    A float variable in which to store the x-coordinate in, passed by reference.
</param>
<param name="y">
    A float variable in which to store the y-coordinate in, passed by reference.
</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native GetHeading(&Float:x, &Float:y);

/** ************************************************************************ **\
<summary>Clears the stack in which the traversing path is stored.</summary>
<returns>
    Returns true on success; False otherwise(i.e. the stack is already empty).
</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native ClearPathStack();

/** ************************************************************************ **\
<summary>Pops a node from the traversing path stack.</summary>
<param name="x">
    A float variable in which to store the x-coordinate in, passed by reference.
    </param>
<param name="y">
    A float variable in which to store the y-coordinate in, passed by reference.
</param>
<returns>
    Returns true on success; False otherwise(i.e. the stack is empty).
</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native PopPathNode(&Float:x, &Float:y);

/** ************************************************************************ **\
<summary>Pushes a node to the traversing path stack.</summary>
<param name="x">The x-coordinate to to push.</param>
<param name="y">The y-coordinate to to push.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native PushPathNode(Float:x, Float:y);

/** ************************************************************************ **\
<summary>Peeks at the next node on the traversing path stack.</summary>
<param name="x">
    A float variable in which to store the x-coordinate in, passed by reference.
</param>
<param name="y">
    A float variable in which to store the y-coordinate in, passed by reference.
</param>
<returns>
    Returns true on success; False otherwise(i.e. the stack is empty).
</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native PeekPathNode(&Float:x, &Float:y);

/** ************************************************************************ **\
<summary>
    Calculates the path from the start coordinates to the end coordinates using
    the A* algorithm.
</summary>
<param name="name">The graph in which to calculate the path.</param>
<param name="xstart">The x-coordinate to start from.</param>
<param name="ystart">The y-coordinate to start from.</param>
<param name="xend">The x-coordinate to move to.</param>
<param name="yend">The y-coordinate to move to.</param>
<returns>
    Returns true on success; False othewise(i.e. no path to the end note has
    been found).
</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native PushPath(const name[], Float:xstart, Float:ystart, Float:xend, Float:yend);

/** ************************************************************************ **\
<summary>Gets the number of nodes in the path stack.</summary>
<returns>The number of nodes in the path stack.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native GetPathStackSize();

/** ************************************************************************ **\
<summary>Sets the model drawn at the position of this agent.</summary>
<param name="value">
    The path to the model to draw at the position of this agent.
</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native SetModel(const value[]);

/** ************************************************************************ **\
<summary>Sets the mass of this agent.</summary>
<param name="value">The mass of this agent.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native SetMass(Float:value);

/** ************************************************************************ **\
<summary>Gets the mass of this agent.</summary>
<returns>The mass of this agent.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native Float:GetMass();

/** ************************************************************************ **\
<summary>Sets the maximum speed of this agent.</summary>
<param name="value">The maximum speed this agent.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native SetMaxSpeed(Float:value);

/** ************************************************************************ **\
<summary>Gets the maximum speed of this agent.</summary>
<returns>The maximum speed of this agent.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native Float:GetMaxSpeed();

/** ************************************************************************ **\
<summary>Sets the maximum force of this agent.</summary>
<param name="value">The maximum force of this agent.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native SetMaxForce(Float:value);

/** ************************************************************************ **\
<summary>Gets the maximum force of this agent.</summary>
<returns>The the maximum of this agent.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native Float:GetMaxForce();

/** ************************************************************************ **\
<summary>Sets the size of this agent.</summary>
<param name="value">The size of this agent.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native SetSize(Float:value);

/** ************************************************************************ **\
<summary>Gets the size of this agent.</summary>
<returns>The size of this agent.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native Float:GetSize();

/** ************************************************************************ **\
<summary>Sets the target range of this agent.</summary>
<param name="value">The target range of this agent.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native SetTargetRange(Float:value);

/** ************************************************************************ **\
<summary>Gets the target range of this agent.</summary>
<returns>The target range of this agent.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native Float:GetTargetRange();

/** ************************************************************************ **\
<summary>
* Adds the specified steering behavior to the queue. This behavior will be
* executed every update in combination with all other behaviors in the queue.
* The added steering behavior will have the specified weight.
* </summary>
<param name="key">
    The key of the steering behavior to add from the queue.
</param>
<param name="type">The type of the steering behavior.</param>
<param name="weight">The weight of the steering behavior.</param>
<param name="x">The targeted x-coordinate of the steering behaviour.</param>
<param name="y">The targeted y-coordinate of the steering behaviour.</param>
<returns>True if successful; False otherwise (i.e. key was not found).</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native AddSteeringBehavior(const key[], type, Float:weight, Float:x=0.0,
    Float:y=0.0);

/** ************************************************************************ **\
<summary>
    Removes the steering behavior with the specified key from the queue.
</summary>
<param name="key">
    The key of the steering behavior to remove from the queue.
</param>
<returns>True if successful; False otherwise (i.e. key was not found).</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native RemoveSteeringBehavior(const key[]);

/** ************************************************************************ **\
<summary>
    Checks whether this agent is within the specified range of the specified
    point.
</summary>
<param name="x">The x-coordinate of the point.<param>
<param name="x">The y-coordinate of the point.<param>
<param name="range">The range.<param>
<returns>
    True if this agent is within the specified range; False otherwise.
</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native IsInRangeOfPoint(Float:x, Float:y, Float:range);

/** ************************************************************************ **\
<summary>
    Checks whether this agent is within the target range of the specified point.
</summary>
<param name="x">The x-coordinate of the point.<param>
<param name="x">The y-coordinate of the point.<param>
<returns>
    True if this agent is within the target range; False otherwise.
</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native IsInTargetRangeOfPoint(Float:x, Float:y);

/** ************************************************************************ **\
<summary>Gets the distance to the specified point.</summary>
<param name="x">The x-coordinate of the point.<param>
<param name="x">The y-coordinate of the point.<param>
<returns>The distance to the specified point.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native Float:GetDistanceToPoint(Float:x, Float:y);

/** ************************************************************************ **\
<summary>Toggles drawing of path.</summary>
<param name="toggle">If true path will be drawn.</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native SetDrawPath(toggle);

/** ************************************************************************ **\
<summary>Gets whether path is being drawn.</summary>
<returns>If true path will be drawn; False otherwise.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native GetDrawPath();

/** ************************************************************************ **\
<summary>Adds a goal to this agent.</summary>
<param name="scriptname">
    Name of the script within the 'goal' folder to run for the goal.
</param>
<param name="format">
    The parameter names and types to be set after creation.
</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native AddGoal(const scriptname[], const format[]='''', {Float,_}:...);

/** ************************************************************************ **\
<summary>Counts the number of goals on the stack.</summary>
<param name="includingSubgoals">If true include subgoals in the count.</param>
<returns>The number of goals on the stack.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native GetGoalCount(includingSubgoals=1);

/** ************************************************************************ **\
<summary>Sets the sound effect played at the position of this agent.</summary>
<param name="sound">Path to the sound effect.</param>
<param name="isLooped">Whether to loop the sound effect.</param>
<param name="volume">The volume of the sound effect.</param>
<returns>
    True if sound effect is successfully added; False otherwise (i.e. the
    specified sound does not exist).
</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native SetSoundEffect(const sound[], isLooped=false, Float:volume=1.0);

/** ************************************************************************ **\
<summary>Removes the set sound effect of this agent.</summary>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within an agent script.</remarks>
\** ************************************************************************ **/
native RemoveSoundEffect();

/** ************************************************************************ **\
<summary>This callback is called every time this actor gets updated.</summary>
<returns>This callback does not handle return values.</returns>
\** ************************************************************************ **/
forward OnUpdate();

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

/** ************************************************************************ **\
<summary>This callback is called when the user clicks this agent.</summary>
<param name="button">The pressed button.</param>
<param name="x">The x-coordinate of the clicked position.</param>
<param name="y">The y-coordinate of the clicked position.</param>
<returns>This callback does not handle return values.</returns>
\** ************************************************************************ **/
forward OnClicked(button, Float:x, Float:y);

/** ************************************************************************ **\
<summary>This callback is called a message was sent to this agent.</summary>
<param name="message">The message.</param>
<param name="contents">The message contents.</param>
<returns>This callback does not handle return values.</returns>
\** ************************************************************************ **/
forward OnIncomingMessage(message, contents);