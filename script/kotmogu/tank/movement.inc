#include "tank/common/team"

#define STEERING_BEHAVIORS      (3)

new static
    ismovementenabled,
    SB:avoidobstacles,
    SB:behaviors[STEERING_BEHAVIORS];

MovementSet()
{
    avoidobstacles = AddAvoidObstacles(1.8);
}

MovementUnset()
{
    if(avoidobstacles != SB:-1)
        RemoveSteeringBehavior(avoidobstacles);

    avoidobstacles = SB:-1;
}

// Cross-script hook.
forward ToggleMovementBehaviors_(toggle);
public ToggleMovementBehaviors_(toggle)
{
    if(ismovementenabled == toggle)
        return;

    if(toggle)
    {
        behaviors[0] = AddAlignmentByVar(0.2, 6.0, "team", GetTeam());
        behaviors[1] = AddCohesionByVar(0.4, 6.0, "team", GetTeam());
        behaviors[2] = AddSeparationByVar(0.7, 6.0, "team", GetTeam());
    }
    else
    {
        for(new i = 0 ; i < STEERING_BEHAVIORS ; i++)
            RemoveSteeringBehavior(behaviors[i]);
    }

    ismovementenabled = toggle;
}
