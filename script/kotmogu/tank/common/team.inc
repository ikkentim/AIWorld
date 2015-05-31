stock GetTeam()
    return GetVar("team");

stock IsAlive()
    return GetVar("team") != 0;

stock GetOpponent()
{
    switch(GetTeam())
    {
        case 1: return 2;
        case 2: return 1;
    }
    return 0;
}

stock GetTeamColor()
{
    switch(GetTeam())
    {
        case 1: return COLOR_RED;
        case 2: return COLOR_YELLOW;
    }
    return COLOR_WHITE;
}
