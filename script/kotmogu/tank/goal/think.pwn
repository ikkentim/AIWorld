/*
 * This goal calculates a logical goal to push to the stack to work on next.
 *
 */
#include <a_goal>
#include "../../common/area"
#include "../common/fuzzy"

main()
{
    LoadFuzzy();
}

public OnUpdate(Float:elapsed)
{
    if(GetSubgoalCount()) return;
    
    SetFuzzyVariables();

    new Float:want_orb = GetOrbDesirability();
    new Float:want_powerup = GetPowerupDesirability();
    new Float:want_carepackage = GetCarepackageDesirability();
    new Float:want_defend = GetDefendDesirability();
    new Float:want_atack = GetAtackDesirability();
    new Float:want_hold = GetHoldDesirability();

    #define BEST_DESIRABILITY(%1) (%1 >= want_powerup && \
        %1 >= want_carepackage && %1 >= want_defend && \
        %1 >= want_atack && %1 >= want_hold)

    if(BEST_DESIRABILITY(want_orb))
        AddSubgoal("kotmogu/tank/goal/get_orb");
    else if(BEST_DESIRABILITY(want_powerup))
        AddSubgoal("kotmogu/tank/goal/powerup");
    else if(BEST_DESIRABILITY(want_carepackage))
        AddSubgoal("kotmogu/tank/goal/get_carepackage");
    else if(BEST_DESIRABILITY(want_hold))
    {
        //logprintf(-1, "Get want_hold");
    }
    else if(BEST_DESIRABILITY(want_defend))
        AddSubgoal("kotmogu/tank/goal/defend");
    else if(BEST_DESIRABILITY(want_atack))
        AddSubgoal("kotmogu/tank/goal/atack");
    else
        logprintf(COLOR_RED, "ERROR: No goal assigned.");
}
