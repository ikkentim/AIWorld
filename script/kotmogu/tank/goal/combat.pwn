/*
 * This goal fights with all enemies nearby untill it runs out of ammo, health
 * or enemies.
 *
 */

#include <a_goal>
#include "../common/combat"
#include "../common/turret"
#include "../common/status"

//TODO: improve so the tank can see more than just the fire range.
// If needed he'll then just drive towards the target.

new targetid;

main() { }

FindTarget()
    targetid = GetEnemyInFireRange();

public OnEnter()
{
    FindTarget();

    if(targetid < 0)
    {
        Terminate();
        return;
    }
}

public OnUpdate(Float:elapsed)
{
    if(GetSubgoalCount()) return;

    // Check exit states
    if(!IsReadyForCombat())
    {
        logprintf(-1, "Terminate combat due to insufficient stats");
        Terminate();
        return;
    }

    new Float:x,
        Float:y,
        Float:tx,
        Float:ty;

    GetPosition(x, y);
    GetEntityPosition(targetid, tx, ty);
    if(GetAgentVar(targetid, "team") == 0 || fdist(x, y, tx, ty) > FIRE_RANGE)
    {
        logprintf(-1, "Target is gone. %d %f", GetAgentVar(targetid, "team"), fdist(x, y, tx, ty));
        FindTarget();

        if(targetid < 0)
        {
            logprintf(-1, "No new target was found, exit combat mode.");
            Terminate();
            return;
        }
    }
    GetEntityPosition(targetid, tx, ty);

    if(TurretIsAimingAt(tx, ty, GetEntitySize(targetid)))
    {
        if(TurretIsReady())
        {
            TurretFire();
        }
    }
    else
    {
        TurretAim(tx, ty);
    }
}
