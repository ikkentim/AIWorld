#define TURRET_STATE_IDLE               (0)
#define TURRET_STATE_AIM                (1)
#define TURRET_STATE_POST_FIRE          (2)
#define TURRET_STATE_AWAIT              (4)

stock TurretStopAiming()
    if(TurretIsAiming())
        TurretSetState(TurretGetState() ^ TURRET_STATE_AIM);

stock TurretIsReady()
    return !(TurretGetState() & TURRET_STATE_POST_FIRE);

stock TurretIsAiming()
    return !!(TurretGetState() & TURRET_STATE_AIM);

stock TurretGetState()
    return CallLocalFunction("TurretGetState_", "");

stock TurretSetState(value)
    return CallLocalFunction("TurretSetState_", "d", value);

stock TurretIsAimingAt(Float:targetx, Float:targety, Float:targetrange)
    return CallLocalFunction("TurretIsAimingAt_", "fff", targetx, targety,
        targetrange);

stock TurretAim(Float:targetx, Float:targety)
    return CallLocalFunction("TurretAim_", "fff", targetx, targety);

stock TurretFire()
    return CallLocalFunction("TurretFire_", "");
