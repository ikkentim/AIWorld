/*
 * This goal gets the nearest orb.
 *
 */
#include <a_goal>

#include "../common/status"
#include "../common/combat"

new static
    SB:seek = SB:-1,
    targetid,
    hasbeenincombat;

main() { }

public OnEnter()
{
    if(hasbeenincombat)
    {
        Terminate();
        return;
    }

    new Float:x, Float:y;
    GetPosition(x, y);
    targetid = GetNearestEnemy();
    if(targetid < 0)
    {
        logprintf(COLOR_RED,"ERROR: Could not find enemy to atack.");
        Terminate();
        return;
    }

    UpdateStatus("Going to atack");

    GetEntityPosition(targetid, x, y);
    seek = AddSeek(0.5, x, y);
}


public OnUpdate(Float:elapsed)
{
    if(AttackIfEnemyNearby())
    {
        UpdateStatus("Entering combat mode");
        hasbeenincombat=true;
        return;
    }

    new Float:x, Float:y;
    GetPosition(x, y);
    new newtargetid = GetNearestEnemy();

    if(newtargetid != targetid)
    {
        if(newtargetid < 0)
        {
            logprintf(COLOR_RED,"ERROR: Could not find enemy to atack.");
            Terminate();
            return;
        }

        GetEntityPosition(targetid, x, y);

        RemoveSteeringBehavior(seek);
        seek = AddSeek(0.5, x, y);
    }
}

public OnExit()
{
    if(_:seek != -1)
        RemoveSteeringBehavior(seek);
    seek = SB:-1;
}
