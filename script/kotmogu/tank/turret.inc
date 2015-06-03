#include <a_sound>
#include <a_drawable>
#include "tank/common/turret"

#define TURRET_SPEED                    (PI/6)

#define TURRET_POST_ROTATE_DELAY        (0.8)
#define TURRET_POST_FIRE_DELAY          (0.8)
#define TURRET_POST_FIRE_EXTRA_DELAY    (1.2)

#define TURRET_BARREL_SLIDE             (0.25)
#define TURRET_CANNON_SPEED             (25)

new static
    turretState,
    Float:turretRotation,
    Float:turretTargetX,
    Float:turretTargetY,
    Float:turretDelay;

TurretInit()
{
    turretState = TURRET_STATE_IDLE;
    turretRotation = 0.0;
}

TurretUpdate(Float:elapsed)
{
    new Float:x,
        Float:y;
    // Based on the current state of the turret, perform some update actions.

    // If the turret is in a waiting state, update the timer.
    if(turretState & TURRET_STATE_AWAIT)
    {
        turretDelay-=elapsed;

        if(turretDelay <= 0)
            turretState ^= TURRET_STATE_AWAIT;
    }

    // If the turret is in a post-fire state, update the animation of the
    // barrel.
    // The post-fire amination is a recoil effect on the barrel of the turret.
    if (turretState & TURRET_STATE_POST_FIRE)
    {
        // If the timer has elapsed, leave this state.
        if(turretDelay <= 0)
            turretState ^= TURRET_STATE_POST_FIRE;

        // If the animation time has elapsed, reset the barrel position.
        // Otherwise update the position of the barrel.
        if(turretDelay < TURRET_POST_FIRE_EXTRA_DELAY)
            SetMeshTranslation("barrel", 0, 0, 0);
        else
        {
            // Calculate the distance the barrel needs to be slid back.
            new Float:slide = (turretDelay - TURRET_POST_FIRE_EXTRA_DELAY) /
                TURRET_POST_FIRE_DELAY * TURRET_BARREL_SLIDE;

            // Divide the slide length into a vector based on the turret
            // rotation.
            x = -floatsin(turretRotation) * slide;
            y = -floatcos(turretRotation) * slide;
            SetMeshTranslation("barrel", x, 0, y);
        }
    }

    // If the turret is in an aiming state, update the rotation of the turret.
    if (turretState & TURRET_STATE_AIM)
    {
        // Calculate the local point of the target
        PointToLocal(turretTargetX, turretTargetY, x, y);

        // Calculate to offset rotation of the turret to the target position.
        new Float:offset = turretRotation - floatatan2(y, x);
        if(offset < -PI) offset += 2 * PI;
        if(offset > PI) offset -= 2 * PI;

        // Update the rotation of the turret.
        turretRotation -= fclamp(-TURRET_SPEED * elapsed,
            TURRET_SPEED * elapsed, offset);

        // Update the rotation of the barrel and turret mesh of the tank.
        SetMeshRotation("head", 0, turretRotation, 0);  // turret.
        SetMeshRotation("barrel", 0, turretRotation, 0);// barrel.
    }

}


// Cross-script hook.
forward TurretGetState_();
public TurretGetState_()
    return turretState;

// Cross-script hook.
forward TurretSetState_(value);
public TurretSetState_(value)
    return turretState = value;

// Cross-script hook.
forward TurretIsAimingAt_(Float:targetx, Float:targety, Float:targetoffset);
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

// Cross-script hook.
forward TurretAim_(Float:targetx, Float:targety);
public TurretAim_(Float:targetx, Float:targety)
{
    // Update the target position and the state.
    turretTargetX = targetx;
    turretTargetY = targety;
    turretState |= TURRET_STATE_AIM;
}

// Cross-script hook.
forward TurretFire_();
public TurretFire_()
{
    new Float:x,
        Float:y,
        Float:hx,
        Float:hy;

    // If the turret is ready for firing, continue.
    if((turretState & TURRET_STATE_POST_FIRE) || GetVar("ammo") == 0)
        return false;

    // Set the delay before the turret can fire again. Update the state of
    // the turret accordingly.
    turretDelay = TURRET_POST_FIRE_DELAY + TURRET_POST_FIRE_EXTRA_DELAY;
    turretState |= TURRET_STATE_POST_FIRE | TURRET_STATE_AWAIT;

    // Notify the tank script and it's goals.
    CallLocalFunction("OnTankFired", "ff", turretTargetX, turretTargetY);

    // Get the position of the tank.
    GetPosition(x, y);

    // Calculate the heading of the turret in world space.
    hx = floatcos(turretRotation);
    hy = floatsin(turretRotation);
    VectorToWorld(hx, hy, hx, hy);

    //Spawn the projectile.
    SpawnProjectile("models/cannon", 45.0, 2.0, x, 0.45, y,
        hx * TURRET_CANNON_SPEED, hy * TURRET_CANNON_SPEED,
        0.01, 0.01, 0.01);

    //TODO: (optional) Add some extra effects of recoil to the tank.

    // Play the fire sound of the turret.
    PlaySound("sounds/turret_fire", 0.8, x, y);

    // Reduce the ammo by 1.
    SetVar("ammo", GetVar("ammo") - 1);

    return true;
}
