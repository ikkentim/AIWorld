#include <a_goal>

/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of the goal.</summary>
\**--------------------------------------------------------------------------**/
main()
{
    //
}

public OnEnter()
{
    logprintf(COLOR_WHITE, "Think.OnEnter");

    //
}

public OnUpdate()
{
    logprintf(COLOR_WHITE, "Think.OnUpdate");

    //TODO: Add some state checking...
    chatprintf(COLOR_WHITE, "CAR: I think I should explore this area...");
    AddSubgoal("car/explore");
}

public OnExit()
{
    logprintf(COLOR_WHITE, "Think.OnExit");

    //
}

public OnIncomingMessage(message, contents)
{
    //
}
