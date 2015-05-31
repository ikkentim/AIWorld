#include <a_agent>
#include <a_drawable>
#include <math>
#include "tank/common/status"
#include "tank/common/team"

#define LABEL_FADE_TIME     (1.0)
#define LABEL_VISIBLE_TIME  (5.0)

new static Drawable:label;
new static Float:labelVisible;
new static isLabelVisible;

forward OnStatusChanged(status[]);
public OnStatusChanged(status[])
{
    chatprintf(GetTeamColor(), status);
    ShowDrawable(label);
    SetDrawableText(label, status);

    new Float:x, Float:y;
    GetPosition(x, y);
    SetDrawablePosition(label, x, 1, y);

    labelVisible = 0;
    isLabelVisible = true;
}

StatusInit()
{
    label = CreateDrawableText3D(0, 0, 0, GetTeamColor(), "fonts/consolas", "");
    SetDrawableScale(label, 0.8, 0.8);
}

StatusUpdate(Float:elapsed)
{
    if(!isLabelVisible) return;

    labelVisible += elapsed;

    if(labelVisible >= LABEL_VISIBLE_TIME)
    {
        HideDrawable(label);
        isLabelVisible = false;
        return;
    }

    new Float:fadePerc = fmin(LABEL_FADE_TIME,
        LABEL_VISIBLE_TIME - labelVisible) / LABEL_FADE_TIME;

    new visible_byte = floatround(fadePerc * 255, floatround_floor);

    new Float:x, Float:y;
    GetPosition(x, y);
    SetDrawablePosition(label, x, 2 - fadePerc, y);
    SetDrawableColor(label, GetTeamColor() | visible_byte);
}
