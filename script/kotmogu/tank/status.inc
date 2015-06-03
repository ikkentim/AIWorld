#include <a_agent>
#include <a_drawable>
#include <math>
#include "tank/common/status"
#include "tank/common/team"

#define LABEL_FADE_TIME     (1.0)
#define LABEL_VISIBLE_TIME  (5.0)

new static
    isLabelVisible,
    Drawable:label,
    Float:labelVisible;

StatusInit()
{
    // Create the status label.
    label = CreateDrawableText3D(0, 0, 0, GetTeamColor(), "fonts/consolas", "");
    SetDrawableScale(label, 0.6, 0.6);
}

StatusUpdate(Float:elapsed)
{
    // If the label should be visible, update the position and effects.
    if(!isLabelVisible) return;

    // Update the visibility timer. If the time elapses the LABEL_VISIBLE_TIME,
    // hide the status label.
    labelVisible += elapsed;
    if(labelVisible >= LABEL_VISIBLE_TIME)
    {
        HideDrawable(label);
        isLabelVisible = false;
        return;
    }

    // Calculate the opacity percentage based on the visiblity timer.
    new Float:fadePerc = fmin(LABEL_FADE_TIME,
        LABEL_VISIBLE_TIME - labelVisible) / LABEL_FADE_TIME;

    // Update the position of the label based on the opacity. The more opaque,
    // the lower the label.
    new Float:x, Float:y;
    GetPosition(x, y);
    SetDrawablePosition(label, x, 2 - fadePerc, y);

    // Update the color of the status label based on the opacity.
    SetDrawableColor(label, GetTeamColor() |
        floatround(fadePerc * 255, floatround_floor));
}

// Cross-script hook.
forward OnStatusChanged(status[]);
public OnStatusChanged(status[])
{
    // Print the status to the chat.
    chatprintf(GetTeamColor(), status);

    // Show and update the label.
    ShowDrawable(label);
    SetDrawableText(label, status);

    labelVisible = 0;
    isLabelVisible = true;
}
