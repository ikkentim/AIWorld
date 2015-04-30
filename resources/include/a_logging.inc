#if defined _logging_included
  #endinput
#endif
#define _logging_included

native log(message[]);
native logf(format[], {Float, _}:...);

native logprintf(color, format[], {Float,_}:...);
