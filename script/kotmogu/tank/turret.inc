#define TURRET_SPEED                    (PI/6)
#define MAX_TURRET_OFFSET               (PI/512)

#define TURRET_POST_ROTATE_DELAY        (0.8)
#define TURRET_POST_FIRE_DELAY          (0.8)
#define TURRET_POST_FIRE_EXTRA_DELAY    (1.2)

#define TURRET_BARREL_SLIDE             (0.25)
#define TURRET_CANNON_SPEED             (25)

#define TURRET_STATE_IDLE               (0)
#define TURRET_STATE_AIM                (1)
#define TURRET_STATE_POST_FIRE          (2)
#define TURRET_STATE_AWAIT              (4)

new turretState = TURRET_STATE_IDLE;
new Float:turretRotation = 0.0;
new Float:turretTargetX;
new Float:turretTargetY;
new Float:turretDelay;

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

        new Float:targetrot = floatatan2(y, x)-turretRotation;

        turretRotation += fclamp(-TURRET_SPEED * elapsed, TURRET_SPEED * elapsed, targetrot);

        SetMeshRotation("head", 0, turretRotation, 0);
        SetMeshRotation("barrel", 0, turretRotation, 0);
    }

}

stock TurretStopAiming()
{
    if(TurretIsAiming())
        turretState ^= TURRET_STATE_AIM;
}

stock TurretIsReady()
{
    return !(turretState & TURRET_STATE_POST_FIRE);
}

stock TurretIsAiming()
{
    return !!(turretState & TURRET_STATE_AIM);
}

stock TurretIsAimingAt(Float:targetx, Float:targety)
{
    new Float:x, Float:y;
    PointToLocal(targetx, targety, x, y);

    new Float:targetrot = floatatan2(y, x)-turretRotation;

    return targetrot > -MAX_TURRET_OFFSET && targetrot < MAX_TURRET_OFFSET;
}

stock TurretAim(Float:targetx, Float:targety)
{
    turretTargetX=targetx;
    turretTargetY=targety;
    turretState |= TURRET_STATE_AIM;
}

stock TurretFire()
{
    if(TurretIsReady())
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
        SpawnProjectile("models/cannon", 0.1, 1.0, x, 0.45, y, vx * TURRET_CANNON_SPEED, vy * TURRET_CANNON_SPEED, 0.01, 0.01, 0.01);
    }
}
