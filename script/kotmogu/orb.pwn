#include <a_agent>
#include <a_drawable>
#include "tank/common/area"
/*
| === VARIABLES TABLE ===
| name      | type  | description
|:---------:|:-----:|:---------------
| carier    | int   | carier tank identifier.
| team      | int   | team identifier. (0:none, 1:axis, 2:alies)
| =======================
*/

new Drawable:model;
new Float:animationtime;
new Float:holdTime=0.0;
new Float:totalHoldTime=0.0;
new Float:originx;
new Float:originy;
new carier = -1;


main()
{
    SetVar("carier", -1);
    SetVar("team", 0);
    SetSize(0);

    model = CreateDrawableLineSphere(1, 1, 1, 0.1, COLOR_YELLOW, COLOR_YELLOW);
    ShowDrawable(model);

    GetPosition(originx, originy);
}

public OnUpdate(Float:elapsed)
{
    animationtime += elapsed;
    if(animationtime > PI * 2)
        animationtime -= PI * 2;

    new Float:x, Float:y;
    GetPosition(x, y);
    SetDrawablePosition(model, x, 0.5 + floatsin(animationtime) * 0.25 + (carier == -1 ? 0.0 : 0.5), y);

    if(carier == -1)
    {
        new tank = FindNearestAgent(x, y, 5, "kotmogu/tank");
        if(tank >= 0)
        {
            new Float:tx, Float:ty;
            GetEntityPosition(tank, tx, ty);
            if(floatsqroot(floatabs(x-tx)*floatabs(x-tx)+floatabs(y-ty)*floatabs(y-ty)) < GetEntitySize(tank) + 0.2)
            {
                carier = tank;
                SetVar("carier", tank);
                SetVar("team", GetAgentVar(tank, "team"));
                if(GetAgentVar(tank, "team") == 1)
                    SetGVar("orbs_axis", GetGVar("orbs_axis") + 1);
                else
                    SetGVar("orbs_alies", GetGVar("orbs_alies") + 1);
                SetAgentVar(tank, "orb", GetId());
                chatprintf(COLOR_YELLOW, "An orb was picked up!");

                holdTime=0;
                totalHoldTime=0;
            }
        }
    }
    else
    {
        holdTime += elapsed;
        totalHoldTime += elapsed;
        if(GetAgentVar(carier, "team") == 0) // Tank is dead
        {
            SetPosition(originx, originy);
            SetVar("carier", carier = -1);
            SetVar("team", 0);
            chatprintf(COLOR_BLUE, "Lost orb after %f seconds!", totalHoldTime);
        }
        else
        {
            GetEntityPosition(carier, x, y);
            SetPosition(x, y);

            if(holdTime > 5.0)
            {
                new score = GetAreaPower(x, y);
                holdTime -= 5.0;
                if(GetVar("team") == 1)
                    SetGVar("score_axis", GetGVar("score_axis") + score);
                else
                    SetGVar("score_alies", GetGVar("score_alies") + score);
            }
        }
    }
}
