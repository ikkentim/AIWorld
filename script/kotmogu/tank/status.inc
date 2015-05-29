#include <a_agent>
#include <a_drawable>
#include <math>
#include "tank/common/status"

#define LABEL_FADE_TIME     (1.0)
#define LABEL_VISIBLE_TIME  (5.0)

new Drawable:label;
new Float:labelVisible;
new isLabelVisible;

forward OnStatusChanged(status[]);
public OnStatusChanged(status[])
{
    chatprintf(COLOR_BLUE, status);
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
    label = CreateDrawableText3D(0, 0, 0, COLOR_BLUE, "fonts/consolas", "");
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
    SetDrawableColor(label, 0x0000ff00 | visible_byte);
}
