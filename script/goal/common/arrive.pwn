#include <a_goal>

public x = 0;
public y = 0;
public weight = 0;

new SB:arrive;

/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of the goal.</summary>
\**--------------------------------------------------------------------------**/
main()
{
    //
}

public OnEnter()
{
    arrive = AddArrive(weight == 0 ? 0.78 : Float:weight, Float:x, Float:y);
}

public OnUpdate(Float:elapsed)
{
    if(IsInTargetRangeOfPoint(Float:x, Float:y))
    {
        Terminate();
    }
}

public OnExit()
{
    RemoveSteeringBehavior(arrive);
}

public OnIncomingMessage(message, contents)
{
    //
}
