#include <a_agent>

new isCtrlDown = false;

public test = -1;

/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of this agent.</summary>
\**--------------------------------------------------------------------------**/
main()
{
    logprintf(COLOR_WHITE, "[car] Test is %f", test);
    // Set agent properties
    SetModel("models/car");
    SetSize(0.3);
    SetMaxForce(15);
    SetMaxSpeed(1.2);
    SetMass(0.35);
    SetTargetRange(0.75);

    AddGoal("car/think");
}

// Currently no update logic.
/*public OnUpdate()
{
    logprintf(COLOR_WHITE, "[car] Updated");
}*/

public OnKeyStateChanged(newKeys[], oldKeys[])
{
    isCtrlDown = IsKeyDown(newKeys, KEY_LEFTCONTROL);

    if(IsKeyDown(newKeys, KEY_F8) != GetDrawPath())
        SetDrawPath(IsKeyDown(newKeys, KEY_F8));
}

public OnMouseClick(button, Float:x, Float:y)
{
    if(button == 1 && isCtrlDown)
    {
        logprintf(COLOR_WHITE, "[car] left clicked at %f %f", x, y);
        chatprintf(COLOR_WHITE, "CAR: OK! I'll go there.");

        RemoveSteeringBehavior("target");
        AddSteeringBehavior("target", BEHAVIOR_ARRIVE, 0.78, x, y);
    }
}

public OnClicked(button, Float:x, Float:y)
{
    Focus();
    return 1;
}

Focus()
{
    SetTargetEntity(GetId());
}
