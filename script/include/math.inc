 #if defined _math_included
    #endinput
#endif
#define _math_included

#define PI              (3.14159265359)

#define RAD2DEG(%0)     (%0 * (180 / PI))
#define DEG2RAD(%0)     (%0 * (PI / 180))

/**
* <summary>
* Generates a pseudo-random float value.
* </summary>
*
* <returns>
* The generated pseudo-random float value.
* </returns>
*/
native Float:frand();

/**
* <summary>
* Generates a pseudo-random float value between the specified boundaries.
* </summary>
*
* <param name="min">
* The minimum value.
* </param>
* <param name="max">
* The maximum value.
* </param>
*
* <returns>
* The generated pseudo-random float value.
* </returns>
*/
stock Float:frandom(Float:min, Float:max)
{
  return frand() * (max-min) + min;
}
