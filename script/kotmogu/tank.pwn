#include <a_agent>
#include <a_drawable>
#include <a_fuzzy>
#include "../includes/utils"

#include "common/carepackages"
#include "tank/common/movement"
#include "tank/common/team"
#include "tank/turret"
#include "tank/status"
#include "tank/movement"
#include "tank/debug"

// Public (setup) variables
public team;

/*
| === VARIABLES TABLE ===
| name      | type  | description
|:---------:|:-----:|:---------------
| team      | int   | team identifier. (0:dead, 1:axis, 2:alies)
| health    | float | health of the tank. (0-100)
| ammo      | int   | number of shels left.
| orb       | int   | orb identifier. (-1: no orb)
| =======================
*/

#define SIZE            (0.50)
#define MAX_FORCE       (35)
#define MAX_SPEED       (2.5)
#define MASS            (0.85)
#define TARGET_RANGE    (0.75)

#define RESPAWN_TIME    (15.0)

#define SINK_DEPTH      (0.55)

new Float:deathTime,
    Float:spawnx,
    Float:spawny,
    Drawable:name;

main()
{
    InitVariables();

    // Set agent properties
    SetModel("models/tank");
    SetSize(SIZE);
    SetMaxForce(MAX_FORCE);
    SetMaxSpeed(MAX_SPEED);
    SetMass(MASS);
    SetTargetRange(TARGET_RANGE);
    SetIsSolid(true);

    // Set model data
    SetMeshVisible("body", true);
    SetMeshScale("body", 0.2, 0.2, 0.2);
    SetMeshVisible("head", true);
    SetMeshScale("head", 0.2, 0.2, 0.2);
    SetMeshVisible("barrel", true);
    SetMeshScale("barrel", 0.2, 0.2, 0.2);

    // Create the name label
    name = CreateDrawableText3D(0,0,0, GetTeamColor(), "fonts/consolas",
        "Tank");
    SetDrawableScale(name, 0.5, 0.5);
    ShowDrawable(name);

    StatusInit();
    DebugInit();

    GetPosition(spawnx, spawny);
}

InitVariables()
{
    RemoveAllSteeringBehaviors();
    SetVar("team", team);
    SetVarFloat("health", 100.0);
    SetVar("ammo", 25);
    SetVar("orb", -1);

    TurretInit();
    MovementSet();

    ToggleMovementBehaviors(true);

    AddGoal("kotmogu/tank/goal/think");
}

UnsetVariables()
{
    deathTime = 0.0;

    MovementUnset();
    SetVar("team", 0);
    SetVarFloat("health", 0.0);
    ResetGoals();
    RemoveAllSteeringBehaviors();
    AddStop(1.0, 0.7);
}

public OnUpdate(Float:elapsed)
{
    new Float:x, Float:y;
    GetPosition(x, y);

    SetDrawableColor(name, GetTeamColor());
    SetDrawablePosition(name, x, 0.5, y);

    if(GetVar("team") == 0)
    {
        deathTime += elapsed;

        // Animate death.
        if(deathTime > RESPAWN_TIME)
        {
            SetMeshTranslation("body", 0, 0, 0);
            SetMeshTranslation("head", 0, 0, 0);
            SetMeshTranslation("barrel", 0, 0, 0);

            InitVariables();
            SetPosition(spawnx, spawny);
        }
        else
        {
            new Float:depth = (deathTime / RESPAWN_TIME) * SINK_DEPTH;
            SetMeshTranslation("body", 0, -depth, 0);
            SetMeshTranslation("head", 0, -depth, 0);
            SetMeshTranslation("barrel", 0, -depth, 0);
        }
    }
    else
    {
        // Update components
        TurretUpdate(elapsed);
    }

    StatusUpdate(elapsed);
    DebugUpdate();
}

public OnClicked(button, Float:x, Float:y)
{
    DebugOnClicked();

    Focus();
    return true;
}

public OnKeyStateChanged(newKeys[], oldKeys[])
{
    DebugOnKeyStateChanged(newKeys, oldKeys);
}

public OnMouseClick(button, Float:x, Float:y)
{
    DebugOnMouseClick(button, x, y);
}

public OnHit(hitid, Float:damage)
{
    new Float:x,
        Float:y,
        Float:health;

    if(!GetTeam())
        return false;

    // Get the position and health of the tank.
    GetPosition(x, y);
    health = GetVarFloat("health");

    // Update the health of the tank based on the damage.
    SetVarFloat("health", health -= damage);

    PlaySound("sounds/turret_impact", 0.8, x, y);
    chatprintf(COLOR_ORANGE, "Hit for %f => %f.", damage, health);

    if(health <= 0)
    {
        // Die.
        UnsetVariables();
        UpdateStatus("Died!");
    }

    return true;
}
