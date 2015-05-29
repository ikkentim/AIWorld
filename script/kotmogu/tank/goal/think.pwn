#include <a_goal>
#include "../common/area"
#include "../common/fuzzy"
#include "../common/status"
main()
{
    LoadFuzzy();
}

public OnUpdate(Float:elapsed)
{
    /*
    GOALS:
    get orb
    keep orb
    defend
    attack
    get create
    */

    UpdateStatus("Calculating goal...");

    SetFuzzyVariables();

    new Float:want_orb = Defuzzify(orb_desirability);
    new Float:want_powerup = Defuzzify(powerup_desirability);
    new Float:want_carepackage = Defuzzify(carepackage_desirability);
    new Float:want_defend = Defuzzify(defend_desirability);
    new Float:want_atack = Defuzzify(atack_desirability);
    new Float:want_hold = Defuzzify(hold_desirability);

    #define BEST_DESIRABILITY(%1) (%1 >= want_powerup && \
        %1 >= want_carepackage && %1 >= want_defend && \
        %1 >= want_atack && %1 >= want_hold)

    if(BEST_DESIRABILITY(want_orb))
    {
        AddSubgoal("kotmogu/tank/goal/get_orb");
    }
    else if(BEST_DESIRABILITY(want_powerup))
    {
        AddSubgoal("kotmogu/tank/goal/powerup");
    }
    else if(BEST_DESIRABILITY(want_carepackage))
    {
        UpdateStatus("Want to get carepackage, but itn't implemented...");
        logprintf(-1, "Get want_carepackage");
    }
    else if(BEST_DESIRABILITY(want_hold))
    {
        UpdateStatus("Want to hold stance, but itn't implemented...");
        logprintf(-1, "Get want_hold");
    }
    else if(BEST_DESIRABILITY(want_defend))
    {
        AddSubgoal("kotmogu/tank/goal/defend");
    }
    else if(BEST_DESIRABILITY(want_atack))
    {
        UpdateStatus("Want to attack, but itn't implemented...");
        logprintf(-1, "Get want_atack");
    }
    else
    {
        logprintf(COLOR_RED, "ERROR: No goal assigned.");
    }
    //AddSubgoal("kotmogu/tank/goal")
}
