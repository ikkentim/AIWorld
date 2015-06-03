#include <a_agent>

stock UpdateStatus(const status[])
    CallLocalFunction("OnStatusChanged", "s", status);
