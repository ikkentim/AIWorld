#include <a_goal>

#include "../common/team"

#define FIRE_RANGE          (25.0)
#define MIN_COMBAT_HEALTH   (5.0)

stock IsReadyForCombat()
    return GetVar("ammo") > 0;

stock bool:AttackIfEnemyNearby()
{
    if(GetEnemyInFireRange() >= 0 && IsReadyForCombat())
    {
        AddSubgoal("kotmogu/tank/goal/combat");
        return true;
    }

    return false;
}

stock GetEnemyInFireRange()
{
    new Float:x,
        Float:y;

    GetPosition(x, y);

    return FindNearestAgentByVar(x, y, FIRE_RANGE, "team", GetOpponent(),
        "kotmogu/tank");
}

stock GetNearestEnemy()
{
    new Float:x,
        Float:y;

    GetPosition(x, y);

    return FindNearestAgentByVar(x, y, 9999, "team", GetOpponent(),
        "kotmogu/tank");
}
