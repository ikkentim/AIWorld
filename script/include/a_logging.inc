#if defined _logging_included
    #endinput
#endif
#define _logging_included

/** ************************************************************************ **\
<summary>Logs a message to the debug console.</summary>
<param name="message">The message to append to the log.</param>
<returns>This function does not return a specific value.</returns>
<remarks>
    Because the debug console is hooked up to the in-game console, the message
    will also be displayed in the in-game console.
</remarks>
\** ************************************************************************ **/
native log(message[]);

/** ************************************************************************ **\
<summary>Logs a message to the debug console.</summary>
<param name="format">The format of message to append to the log.</param>
<returns>This function does not return a specific value.</returns>
<remarks>
    Because the debug console is hooked up to the in-game console, the message
    will also be displayed in the in-game console.
</remarks>
\** ************************************************************************ **/
native logf(format[], {Float, _}:...);

/** ************************************************************************ **\
<summary>Logs a message to the in-game console.</summary>
<param name="format">The format of message to append to the log.</param>
<returns>This function does not return a specific value.</returns>
\** ************************************************************************ **/
native logprintf(color, format[], {Float,_}:...);
