#if defined _logging_included
    #endinput
#endif
#define _logging_included

/** ************************************************************************ **\
<summary>Logs a message to the in-game console.</summary>
<param name="color">The color of message to append to the log.</param>
<param name="format">The format of message to append to the log.</param>
<returns>This function does not return a specific value.</returns>
\** ************************************************************************ **/
native logprintf(color, format[], {Float,_}:...);

/** ************************************************************************ **\
<summary>Logs a message to the chat.</summary>
<param name="color">The color of message to append to the chat.</param>
<param name="format">The format of message to append to the chat.</param>
<returns>This function does not return a specific value.</returns>
\** ************************************************************************ **/
native chatprintf(color, format[], {Float,_}:...);
