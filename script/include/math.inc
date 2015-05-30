#if defined _math_included
    #endinput
#endif
#define _math_included

#include <core>

#define PI              (3.14159265359)

#define RAD2DEG(%0)     (%0 * (180 / PI))
#define DEG2RAD(%0)     (%0 * (PI / 180))

/**--------------------------------------------------------------------------**\
<summary>Generates a pseudo-random float value.</summary>
<returns>The generated pseudo-random float value.</returns>
\**--------------------------------------------------------------------------**/
native Float:frand();

// float.inc additions
native Float:floatatan2(Float:a, Float:b);

/**--------------------------------------------------------------------------**\
<summary>Generates a pseudo-random float value between the specified boundaries.</summary>
<param name="min">The minimum value.</param>
<param name="max">The maximum value.</param>
<returns>The generated pseudo-random float value.</returns>
\**--------------------------------------------------------------------------**/
stock Float:frandom(Float:min, Float:max)
    return frand() * (max-min) + min;

//TODO: Add docs
stock Float:fclamp(Float:min, Float:max, Float:value)
    return value < min ? min : (value > max ? max : value);
/**--------------------------------------------------------------------------**\
<summary>Gets the absolute value of the specified value.</summary>
<param name="value">The value.</param>
<returns>The absolute value.</returns>
\**--------------------------------------------------------------------------**/
stock Float:fabs(Float:value)
    return value < 0 ? -value : value;

stock Float:fmin(Float:val1, Float:val2)
    return val2 < val1 ? val2 : val1;

stock Float:fmax(Float:val1, Float:val2)
    return val2 > val1 ? val2 : val1;

stock Float:fdist(Float:x1, Float:y1, Float:x2, Float:y2)
{
    new Float:x = fabs(x1 - x2);
    new Float:y = fabs(y1 - y2);

    return floatsqroot(x*x+y*y);
}

stock random2(min, max)
    return min + random(max-min);
