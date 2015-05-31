#define FIRE_RANGE      (25.0)
#define MIN_COMBAT_HEALTH    (5.0)
#include <a_goal>
#include "../common/team"

stock IsReadyForCombat()
    return GetVar("ammo") > 0;

stock AttackIfEnemyNearby()
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
    new id = FindNearestAgentByVar(x, y, FIRE_RANGE, "team", GetOpponent(), "kotmogu/tank");
    return id;
}

stock GetNearestEnemy()
{
    new Float:x,
        Float:y;

    GetPosition(x, y);
    return FindNearestAgentByVar(x, y, 9999, "team", GetOpponent(), "kotmogu/tank");
}
