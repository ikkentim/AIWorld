#if defined _agent_included
    #endinput
#endif
#define _agent_included

/**
* <summary>
* Gets the position of this agent.
* </summary>
*
* <param name="x">
* A float variable in which to store the x-coordinate in, passed by reference.
* </param>
* <param name="y">
* A float variable in which to store the y-coordinate in, passed by reference.
* </param>
*
* <returns>
* This function does not return a specific value.
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native GetPosition(&Float:x, &Float:y);

/**
* <summary>
* Gets the direction this agent is facing.
* </summary>
*
* <param name="x">
* A float variable in which to store the x-coordinate in, passed by reference.
* </param>
* <param name="y">
* A float variable in which to store the y-coordinate in, passed by reference.
* </param>
*
* <returns>
* This function does not return a specific value.
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native GetHeading(&Float:x, &Float:y);

/**
* <summary>
* Clears the stack in which the traversing path is stored.
* </summary>
*
* <returns>
* Returns true on success; False otherwise(i.e. the stack is already empty).
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native ClearPathStack();

/**
* <summary>
* Pops a node from the traversing path stack.
* </summary>
*
* <param name="x">
* A float variable in which to store the x-coordinate in, passed by reference.
* </param>
* <param name="y">
* A float variable in which to store the y-coordinate in, passed by reference.
* </param>
*
* <returns>
* Returns true on success; False otherwise(i.e. the stack is empty).
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native PopPathNode(&Float:x, &Float:y);

/**
* <summary>
* Pushes a node to the traversing path stack.
* </summary>
*
* <param name="x">
* The x-coordinate to to push.
* </param>
* <param name="y">
* The y-coordinate to to push.
* </param>
*
* <returns>
* This function does not return a specific value.
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native PushPathNode(Float:x, Float:y);

/**
* <summary>
* Peeks at the next node on the traversing path stack.
* </summary>
*
* <param name="x">
* A float variable in which to store the x-coordinate in, passed by reference.
* </param>
* <param name="y">
* A float variable in which to store the y-coordinate in, passed by reference.
* </param>
*
* <returns>
* Returns true on success; False otherwise(i.e. the stack is empty).
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native PeekPathNode(&Float:x, &Float:y);

/**
* <summary>
* Calculates the path from the start coordinates to the end coordinates using
* the Dijkstra's algorithm.
* </summary>
*
* <param name="xstart">
* The x-coordinate to start from.
* </param>
* <param name="ystart">
* The y-coordinate to start from.
* </param>
* <param name="xend">
* The x-coordinate to move to.
* </param>
* <param name="yend">
* The y-coordinate to move to.
* </param>
*
* <returns>
* Returns true on success; False othewise(i.e. no path to the end note was
* found).
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native PushPath(Float:xstart, Float:ystart, Float:xend, Float:yend);

/**
* <summary>
* Sets the model drawn at the position of this agent.
* </summary>
*
* <param name="value">
* The path to the model to draw at the position of this agent.
* </param>
*
* <returns>
* This function does not return a specific value.
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native SetModel(const value[]);

/**
* <summary>
* Sets the mass of this agent.
* </summary>
*
* <param name="value">
* The mass of this agent.
* </param>
*
* <returns>
* This function does not return a specific value.
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native SetMass(Float:value);
native Float:GetMass();

/**
* <summary>
* Sets the maximum speed of this agent.
* </summary>
*
* <param name="value">
* The maximum speed this agent.
* </param>
*
* <returns>
* This function does not return a specific value.
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native SetMaxSpeed(Float:value);

/**
* <summary>
* Gets the maximum speed of this agent.
* </summary>
*
* <returns>
* The maximum speed of this agent.
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native Float:GetMaxSpeed();

/**
* <summary>
* Sets the maximum force of this agent.
* </summary>
*
* <param name="value">
* The maximum force of this agent.
* </param>
*
* <returns>
* This function does not return a specific value.
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native SetMaxForce(Float:value);

/**
* <summary>
* Gets the maximum force of this agent.
* </summary>
*
* <returns>
* The the maximum of this agent.
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native Float:GetMaxForce();

/**
* <summary>
* Sets the size of this agent.
* </summary>
*
* <param name="value">
* The size of this agent.
* </param>
*
* <returns>
* This function does not return a specific value.
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native SetSize(Float:value);

/**
* <summary>
* Gets the size of this agent.
* </summary>
*
* <returns>
* The size of this agent.
* </returns>
*
* <remarks>
* This function can only be used within an agent script.
* </remarks>
*/
native Float:GetSize();
