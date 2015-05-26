#include <a_agent>
#include <a_drawable>
#include <a_fuzzy>

// Public (setup) variables
public tx;
public ty;

#define SIZE            (0.50)
#define MAX_FORCE       (15)
#define MAX_SPEED       (0.3)
#define MASS            (0.4)
#define TARGET_RANGE    (1)

new Drawable:collisionBox;
new Drawable:targetLine;

/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of this agent.</summary>
\**--------------------------------------------------------------------------**/
main()
{
    // Set agent properties
    //SetSoundEffect("sounds/engine", true, 0.6);
    SetModel("models/tank");
    SetSize(SIZE);
    SetMaxForce(MAX_FORCE);
    SetMaxSpeed(MAX_SPEED);
    SetMass(MASS);
    SetTargetRange(TARGET_RANGE);

    // Set model data
    SetMeshVisible("body", true);
    SetMeshScale("body", 0.2, 0.2, 0.2);
    SetMeshVisible("head", true);
    SetMeshScale("head", 0.2, 0.2, 0.2);
    SetMeshVisible("barrel", true);
    SetMeshScale("barrel", 0.2, 0.2, 0.2);

    // Enable debug box
    collisionBox = CreateDrawableLineCylinder(0, 0, 0, 0, 1, 0, 0.5, SIZE,
        COLOR_BLACK, COLOR_BLACK);
    targetLine = CreateDrawableLine(0,0.5,0,0,5,0, COLOR_RED, COLOR_RED);
    ShowDrawable(collisionBox);
    //ShowDrawable(targetLine);

    // Add debug goal
    //AddGoal("common/arrive", "x,y", 10.0, 20.0);
    chatprintf(COLOR_WHITE, "Start firing at %f, %f", tx, ty);
    TurretAim(Float:tx, Float:ty);

    // Focus for debugging
    Focus();

    new FuzzyVar:namelength = CreateFuzzyVariable("name_length");
    new FuzzyVar:awesomeness = CreateFuzzyVariable("awesomeness");

    AddFuzzyLeftShoulder(namelength, "short", 1, 4, 6);
    AddFuzzyTriangle(namelength, "medium", 1, 4, 6);
    AddFuzzyRightShoulder(namelength, "long", 6, 8, 50);

    AddFuzzyLeftShoulder(awesomeness, "low", 0, 25, 35);
    AddFuzzyTriangle(awesomeness, "below_normal", 25, 35, 45);
    AddFuzzyTrapezium(awesomeness, "normal", 35, 45, 65, 75);
    AddFuzzyTriangle(awesomeness, "high", 65, 75, 85);
    AddFuzzyRightShoulder(awesomeness, "awesome", 75, 85, 100);

/*
(longName, lowAwesomeness.Fairly());
(longName.Very(), lowAwesomeness.Very());
*/
    AddFuzzyRule("IF very short name_length THEN very awesome awesomeness");
    AddFuzzyRule("IF fairly short name_length AND fairly medium name_length THEN very high awesomeness");
    AddFuzzyRule("IF medium name_length THEN very normal awesomeness AND fairly high awesomeness AND fairly below_normal awesomeness");
    AddFuzzyRule("IF medium name_length OR fairly long name_length THEN below_normal awesomeness AND fairly low awesomeness");
    AddFuzzyRule("IF long name_length THEN fairly low awesomeness");
    AddFuzzyRule("IF very long name_length THEN very low awesomeness");

    Fuzzify(namelength, 7);
    new Float:result = Defuzzify(awesomeness);

    chatprintf(COLOR_YELLOW, "A name length of 7 is %f awesome", result);
}

public OnUpdate(Float:elapsed)
{
    // Update debug box
    new Float:x, Float:y;
    GetPosition(x, y);
    SetDrawablePosition(collisionBox, x, 0, y);

    TurretUpdate(elapsed);

    if(TurretIsAiming() && TurretIsAimingAt(Float:tx, Float:ty) && TurretIsReady())
    {
        TurretFire();
    }
}

public OnHit(hitid, Float: damage)
{
    chatprintf(COLOR_ORANGE, "I was hit for %f damage by #%d!", damage, hitid);
}

Focus()
{
    SetTargetEntity(GetId());
}

 /*============================*
 *        AIMING LOGIC        *
*============================*/

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
        new Float:x, Float:y, Float:px, Float:py;
        PointToLocal(turretTargetX, turretTargetY, x, y);

        new Float:targetrot = floatatan2(y, x)-turretRotation;

        turretRotation += fclamp(-TURRET_SPEED * elapsed, TURRET_SPEED * elapsed, targetrot);

        GetPosition(px, py);
        SetDrawablePosition(targetLine, px, 0.5, py);
        SetDrawablePosition2(targetLine, turretTargetX, 0.5, turretTargetY);

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
        CallPublicFunction("OnTankFired", "ff", turretTargetX, turretTargetY);

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
