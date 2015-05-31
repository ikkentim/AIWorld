new static Drawable:zones[4*4];

DebugInit()
{
    // Create some lines defining the zones
    #define UPLINE(%1,%2,%3) CreateDrawableLine(%1, 0, %2, %1, 1, %2, %3, %3)
    zones[0] = UPLINE(-10, -10, COLOR_PINK);
    zones[1] = UPLINE( 10, -10, COLOR_PINK);
    zones[2] = UPLINE(-10,  10, COLOR_PINK);
    zones[3] = UPLINE( 10,  10, COLOR_PINK);

    zones[4] = UPLINE(-30, -30, COLOR_PINK);
    zones[5] = UPLINE( 30, -30, COLOR_PINK);
    zones[6] = UPLINE(-30,  30, COLOR_PINK);
    zones[7] = UPLINE( 30,  30, COLOR_PINK);

    zones[8] = UPLINE(-50, -50, COLOR_PINK);
    zones[9] = UPLINE( 50, -50, COLOR_PINK);
    zones[10] = UPLINE(-50,  50, COLOR_PINK);
    zones[11] = UPLINE( 50,  50, COLOR_PINK);

    zones[12] = UPLINE(-70, -70, COLOR_PINK);
    zones[13] = UPLINE( 70, -70, COLOR_PINK);
    zones[14] = UPLINE(-70,  70, COLOR_PINK);
    zones[15] = UPLINE( 70,  70, COLOR_PINK);

    for(new i=0;i<16;i++)
        ShowDrawable(zones[i]);
}
