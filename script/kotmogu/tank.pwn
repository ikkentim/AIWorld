#include <a_agent>
#include <a_drawable>
#include <a_fuzzy>
#include "../includes/utils"

#include "common/carepackages"
#include "tank/turret"
#include "tank/status"

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
#define MAX_FORCE       (20)
#define MAX_SPEED       (5.5)
#define MASS            (1.2)
#define TARGET_RANGE    (1)

new Drawable:collisionBox;

main()
{
    SetVar("team", team);
    SetVarFloat("health", 100);
    SetVar("ammo", 0);//25);
    SetVar("orb", -1);

    AddAvoidObstacles(0.9);
    AddGoal("kotmogu/tank/goal/think");

    // Set agent properties
    SetModel("models/tank");
    SetSize(SIZE);
    SetMaxForce(MAX_FORCE);
    SetMaxSpeed(MAX_SPEED);
    SetMass(MASS);
    SetTargetRange(TARGET_RANGE);

    // Set model data
    SetMeshVisible("body", true);
    SetMeshScale("body", 0.2, 0.2, 0.2);
    SetMeshVisible("head", true);
    SetMeshScale("head", 0.2, 0.2, 0.2);
    SetMeshVisible("barrel", true);
    SetMeshScale("barrel", 0.2, 0.2, 0.2);

    // Enable debug box
    collisionBox = CreateDrawableLineCylinder(0, 0, 0, 0, 1, 0, 0.5, SIZE,
        COLOR_BLACK, COLOR_BLACK);
    ShowDrawable(collisionBox);

    StatusInit();

    Focus();
}

public OnUpdate(Float:elapsed)
{
    // Update debug box
    new Float:x, Float:y;
    GetPosition(x, y);
    SetDrawablePosition(collisionBox, x, 0, y);

    // Update componenets
    TurretUpdate(elapsed);
    StatusUpdate(elapsed);
}

public OnClicked(button, Float:x, Float:y)
{
    Focus();
    return 1;
}

public OnHit(hitid, Float: damage)
{
    chatprintf(COLOR_ORANGE, "I was hit for %f damage by #%d!", damage, hitid);
}
