#include <a_drawable>
#include <a_agent>
#include <string>

#include "tank/common/team"

// Different label types.
#define LABEL_POSITION      (0)
#define LABEL_VELOCITY      (1)
#define LABEL_HEADING       (2)
#define LABEL_HEALTH        (3)
#define LABEL_AMMO          (4)
#define LABEL_TEAM          (5)
#define LABEL_STEERING      (6)
#define LABEL_GOALS         (7)

#define LABEL_COUNT         (8)

new static
    isselected,
    Drawable:labels[LABEL_COUNT];

DebugInit()
{
    // Create the debug labels.
    for(new i = 0 ; i < LABEL_COUNT ; i++)
    {
        labels[i] = CreateDrawableText2D(5, 50 + i * 15, COLOR_WHITE,
            "fonts/consolas", "");
        SetDrawableScale(labels[i], 0.6, 0.6);
    }
}

native GetGoalName(index, name[], len = sizeof name);
native GetGoalDepth(index);
native GetSteeringBehaviorName(index, &Float:weight, name[], len = sizeof name);
native GetSteeringBehaviorsCount();

DebugUpdate()
{
    new Float:x,
        Float:y,
        buffer[512],
        namebuffer[64],
        miscbuffer[32];

    // If this tank is selected, update the information in the debug labels.
    if(!isselected) return;

    // Update the position label.
    GetPosition(x, y);
    format(buffer, sizeof(buffer), "Position: (%f, %f)", x, y);
    SetDrawableText(labels[LABEL_POSITION], buffer);

    // Update the velocity label.
    GetVelocity(x, y);
    format(buffer, sizeof(buffer), "Velocity: (%f, %f)", x, y);
    SetDrawableText(labels[LABEL_VELOCITY], buffer);

    // Update the heading label.
    GetHeading(x, y);
    format(buffer, sizeof(buffer), "Heading: (%f, %f)", x, y);
    SetDrawableText(labels[LABEL_HEADING], buffer);

    // Update the health label.
    format(buffer, sizeof(buffer), "Health: %f", GetVarFloat("health"));
    SetDrawableText(labels[LABEL_HEALTH], buffer);

    // Update the ammo label.
    format(buffer, sizeof(buffer), "Ammo: %d", GetVar("ammo"));
    SetDrawableText(labels[LABEL_AMMO], buffer);

    // Update the team label.
    format(buffer, sizeof(buffer), "Team: %d", GetTeam());
    SetDrawableText(labels[LABEL_TEAM], buffer);

    // Update the steering label.
    new steeringCount = GetSteeringBehaviorsCount();
    new Float:weight;
    format(buffer, sizeof(buffer), "");
    for(new i = 0 ; i < steeringCount ; i++)
        if(GetSteeringBehaviorName(i, weight, namebuffer))
            format(buffer, sizeof(buffer), "%s %f:%s", buffer, weight, namebuffer);

    format(buffer, sizeof(buffer), "SBs:%s", buffer);
    SetDrawableText(labels[LABEL_STEERING], buffer);

    // Update the goal label.
    new goalCount = GetGoalCount(true);
    format(buffer, sizeof(buffer), "");
    for(new i = 0 ; i < goalCount ; i++)
        if(GetGoalName(i, namebuffer))
        {
            strrepeat(miscbuffer, _, ' ', 2 * GetGoalDepth(i));
            format(buffer, sizeof(buffer), "%s%s:%s\n", buffer,miscbuffer, namebuffer);
        }

    format(buffer, sizeof(buffer), "Goals:\n%s", buffer);
    SetDrawableText(labels[LABEL_GOALS], buffer);
}

forward OnTankSelected(tankid);
public OnTankSelected(tankid)
{
    // If this tank has been selected, show the labels; otherwise hide them.
    if(tankid == GetId())
    {
        isselected = true;

        for(new i = 0 ; i < LABEL_COUNT ; i++)
            ShowDrawable(labels[i]);
    }
    else
    {
        isselected = false;

        for(new i = 0 ; i < LABEL_COUNT ; i++)
            HideDrawable(labels[i]);
    }
}

DebugOnClicked()
{
    // If this tank was already selected, ignore the call.
    if(isselected) return;

    // Notify all tanks of the change in selection.
    CallPublicFunction("OnTankSelected", "d", GetId());
}
