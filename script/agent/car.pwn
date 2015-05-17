#include <a_agent>

new isCtrlDown = false;

/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of this agent.</summary>
\**--------------------------------------------------------------------------**/
main()
{    // Set agent properties
    SetModel("models/car");
    SetSoundEffect("sounds/engine", true, 0.15);
    SetSize(0.30);
    SetMaxForce(15);
    SetMaxSpeed(1.50);
    SetMass(0.25);
    SetTargetRange(0.95);

    AddSteeringBehavior("obstacleavoidance", BEHAVIOR_OBSTACLE_AVOIDANCE, 0.9);

    AddGoal("car/think");
}

public OnUpdate(Float:elapsed)
{
    //
}

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

        AddGoal("common/seek", "x:f,y:f", x, y);
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
