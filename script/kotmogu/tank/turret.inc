#include <a_sound>
#include <a_drawable>
#include "tank/common/turret"

#define TURRET_SPEED                    (PI/6)

#define TURRET_POST_ROTATE_DELAY        (0.8)
#define TURRET_POST_FIRE_DELAY          (0.8)
#define TURRET_POST_FIRE_EXTRA_DELAY    (1.2)

#define TURRET_BARREL_SLIDE             (0.25)
#define TURRET_CANNON_SPEED             (25)

new static turretState;
new static Float:turretRotation;
new static Float:turretTargetX;
new static Float:turretTargetY;
new static Float:turretDelay;

TurretInit()
{
    turretState = TURRET_STATE_IDLE;
    turretRotation = 0.0;
}

TurretUpdate(Float:elapsed)
{
    if(turretState & TURRET_STATE_AWAIT)
    {
        turretDelay-=elapsed;
        if(turretDelay <= 0)
        {
            turretState ^= TURRET_STATE_AWAIT;
        }
    }

    if (turretState & TURRET_STATE_POST_FIRE)
    {
        if(turretDelay <= 0)
        {
            SetMeshTranslation("barrel", 0, 0, 0);
            turretState ^= TURRET_STATE_POST_FIRE;
        }
        else
        {
            if(turretDelay < TURRET_POST_FIRE_EXTRA_DELAY)
            {
                SetMeshTranslation("barrel", 0, 0, 0);
            }
            else
            {
                new Float:slide = ((turretDelay - TURRET_POST_FIRE_EXTRA_DELAY) / (TURRET_POST_FIRE_DELAY)) * TURRET_BARREL_SLIDE;
                new Float:slidex = -floatsin(turretRotation) * slide;
                new Float:slidey = -floatcos(turretRotation) * slide;
                SetMeshTranslation("barrel", slidex, 0, slidey);
            }
        }
    }

    if (turretState & TURRET_STATE_AIM)
    {
        new Float:x, Float:y;
        PointToLocal(turretTargetX, turretTargetY, x, y);

        new Float:currentRotation = turretRotation;
        new Float:targetRotation = floatatan2(y, x);

        //if(floatabs(currentRotation - ))
        if(currentRotation < 0) currentRotation += PI * 2;
        if(targetRotation < 0) targetRotation += PI * 2;

        new Float:offset = targetRotation - currentRotation;

        turretRotation += fclamp(-TURRET_SPEED * elapsed, TURRET_SPEED * elapsed, offset);

        SetMeshRotation("head", 0, turretRotation, 0);
        SetMeshRotation("barrel", 0, turretRotation, 0);
    }

}

public TurretGetState_()
{
    return turretState;
}

public TurretSetState_(value)
{
    return turretState = value;
}

public TurretIsAimingAt_(Float:targetx, Float:targety, Float:targetoffset)
{
    new Float:x,
        Float:y,
        Float:cx,
        Float:cy,
        Float:length;

    // Calculate the local point of the target.
    PointToLocal(targetx, targety, x, y);

    // Calculate the length of the target vector.
    length = fdist(0, 0, x, y);

    // Calculate the aiming vector with the same length as the target vector.
    cx = floatcos(turretRotation) * length;
    cy = floatsin(turretRotation) * length;

    return fdist(x, y, cx, cy) < targetoffset;
}

public TurretAim_(Float:targetx, Float:targety)
{
    turretTargetX = targetx;
    turretTargetY = targety;
    turretState |= TURRET_STATE_AIM;
}

public TurretFire_()
{
    if(!(turretState & TURRET_STATE_POST_FIRE) &&
        GetVar("ammo") > 0) // Ready for firing...
    {
        turretDelay = TURRET_POST_FIRE_DELAY + TURRET_POST_FIRE_EXTRA_DELAY;
        turretState |= TURRET_STATE_POST_FIRE | TURRET_STATE_AWAIT;

        // Notify everyone
        CallLocalFunction("OnTankFired", "ff", turretTargetX, turretTargetY);

        // Play sound
        new Float:x, Float:y;
        GetPosition(x, y);
        PlaySound("sounds/fire", 0.5, x, y);

        //TODO: Play moved vehicle effect

        //Spawn projectile
        new Float:hx = floatcos(turretRotation);
        new Float:hy = floatsin(turretRotation);

        new Float:vx, Float:vy;
        VectorToWorld(hx, hy, vx, vy);
        SpawnProjectile("models/cannon", 30.0, 1.0, x, 0.45, y, vx * TURRET_CANNON_SPEED, vy * TURRET_CANNON_SPEED, 0.01, 0.01, 0.01);

        SetVar("ammo", GetVar("ammo") - 1);
        return true;
    }

    return false;
}
