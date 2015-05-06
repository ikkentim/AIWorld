#if defined _goal_included
    #endinput
#endif
#define _goal_included

#include <a_agent>

/** ************************************************************************ **\
<summary>Adds a subgoal to this goal.</summary>
<param name="scriptname">
    Name of the script within the 'goal' folder to run for the subgoal.
</param>
<param name="format">
    The parameter names and types to be set after creation.
</param>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within a goal script.</remarks>
\** ************************************************************************ **/
native AddSubgoal(const scriptname[], const format[]='''', {Float,_}:...);

/** ************************************************************************ **\
<summary>Terminates this goal.</summary>
<returns>This function does not return a specific value.</returns>
<remarks>This function can only be used within a goal script.</remarks>
\** ************************************************************************ **/
native Terminate();

/** ************************************************************************ **\
<summary>Gets the name of this goal.</summary>
<param name="name">
    An array variable in which to store the name of this goal.
</param>
<param name="len">The length of the name array.</param>
<returns>The length of the name.</returns>
<remarks>This function can only be used within a goal script.</remarks>
\** ************************************************************************ **/
native GetName(name[], len = sizeof name);

/** ************************************************************************ **\
<summary>
    This callback is called when the agent starts or resumes this goal.
</summary>
<returns>This callback does not handle return values.</returns>
\** ************************************************************************ **/
forward OnEnter();

/** ************************************************************************ **\
<summary>
    This callback is called when the agent terminates or pauses this goal.
</summary>
<returns>This callback does not handle return values.</returns>
\** ************************************************************************ **/
forward OnExit();
