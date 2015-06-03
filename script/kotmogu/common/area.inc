
/*
| Map is divided into multiple areas:
| -10,-10 ~ 10,10   Power 4
| -30,-30 ~ 30,30   Power 3
| -50,-50 ~ 50,50   Power 2
| -70,-70 ~ 70,70   Power 1
| other             Power 0
*/

stock GetAreaPower(Float:x, Float:y)
{
    #define POWER_BOUNDS(%1,%2,%3,%4,%5); if(x>=%1 && y>=%2 && x<=%3 && y<=%4) \
        { return %5; }
        
    POWER_BOUNDS(-10,-10, 10, 10, 4);
    POWER_BOUNDS(-30,-30, 30, 30, 3);
    POWER_BOUNDS(-50,-50, 50, 50, 2);
    POWER_BOUNDS(-70,-70, 70, 70, 1);
    return 0;
}

stock GetNearestAreaForPower(Float:x, Float:y, power, &Float:ox, &Float:oy)
{
    // TODO: This currently only works if you want to power up.
    // If you want to power down it will give faulty results, except for power 0
    // At the moment of writing this code there is however no need for powering
    // down the areas.
    switch(power)
    {
        case 0: {
            if(floatabs(x) > 70 && floatabs(y) > 70)
            {
                ox=x;
                oy=y;
                return 1;
            }

            if(floatabs(x) > 70)
                ox = x;
            else if (ox > 0)
                ox = 75;
            else
                ox = -75;

            if(floatabs(y) > 70)
                oy = y;
            else if (oy > 0)
                oy = 75;
            else
                oy = -75;

            return 1;
        }
        case 1: {
            if(floatabs(x) <= 70 && floatabs(y) <= 70)
            {
                ox=x;
                oy=y;
                return 1;
            }

            if(floatabs(x) <= 70)
                ox = x;
            else if (ox > 0)
                ox = 65;
            else
                ox = -65;

            if(floatabs(y) <= 70)
                oy = y;
            else if (oy > 0)
                oy = 65;
            else
                oy = -65;
            return 1;
        }
        case 2: {
            if(floatabs(x) <= 50 && floatabs(y) <= 50)
            {
                ox=x;
                oy=y;
                return 1;
            }

            if(floatabs(x) <= 50)
                ox = x;
            else if (ox > 0)
                ox = 45;
            else
                ox = -45;

            if(floatabs(y) <= 50)
                oy = y;
            else if (oy > 0)
                oy = 45;
            else
                oy = -45;
            return 1;
        }
        case 3: {
            if(floatabs(x) <= 30 && floatabs(y) <= 30)
            {
                ox=x;
                oy=y;
                return 1;
            }

            if(floatabs(x) <= 30)
                ox = x;
            else if (ox > 0)
                ox = 25;
            else
                ox = -25;

            if(floatabs(y) <= 30)
                oy = y;
            else if (oy > 0)
                oy = 25;
            else
                oy = -25;
            return 1;
        }
        case 4: {
            if(floatabs(x) <= 10 && floatabs(y) <= 10)
            {
                ox=x;
                oy=y;
                return 1;
            }

            if(floatabs(x) <= 10)
                ox = x;
            else if (ox > 0)
                ox = 5;
            else
                ox = -5;

            if(floatabs(y) <= 10)
                oy = y;
            else if (oy > 0)
                oy = 5;
            else
                oy = -5;
            return 1;
        }
    }

    return 0;
}
