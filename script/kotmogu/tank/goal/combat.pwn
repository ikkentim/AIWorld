/*
 * This goal fights with all enemies nearby until it runs out of ammo, health
 * or enemies.
 *
 */

#include <a_goal>
#include "../common/combat"
#include "../common/turret"
#include "../common/status"
#include "../common/movement"

// TODO: improve so the tank can see more than just the fire range.
// If needed he'll then just drive towards the target.
// TODO: Check whether the tank can acutally see the target (CastRay) before
// shooting.

new targetid,
    SB:stop;

main() { }

FindTarget()
{
    targetid = GetEnemyInFireRange();
}

public OnEnter()
{
    FindTarget();

    // Stop the tank for a steadier aim.
    stop = AddStop(1.0, 0.5);
    ToggleMovementBehaviors(false);

    if(targetid < 0)
    {
        Terminate();
        return;
    }
}

public OnUpdate(Float:elapsed)
{
    new Float:x,
        Float:y;

    // If the tank is not ready for combat anymore, terminate this goal.
    if(!IsReadyForCombat())
    {
        Terminate();
        return;
    }

    // If the target is dead or no longer in range, find a new target.
    GetEntityPosition(targetid, x, y);
    if(GetAgentVar(targetid, "team") == 0 ||
        !IsInRangeOfPoint(x, y, FIRE_RANGE))
    {
        FindTarget();

        // If no suitable target was found, terminate this goal.
        if(targetid < 0)
        {
            Terminate();
            return;
        }
    }

    // If the turret is not yet aiming towards the target, update the aim
    // position. Otherwise fire if the turret is ready.
    if(!TurretIsAimingAt(x, y, GetEntitySize(targetid)))
        TurretAim(x, y);
    else if(TurretIsReady())
        TurretFire();
}

public OnExit()
{
    // When exiting, stop the turret from aiming towards the target.
    TurretStopAiming();

    // Remove the stop steering behavior.
    RemoveSteeringBehavior(stop);
    ToggleMovementBehaviors(true);
}
