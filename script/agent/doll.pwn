#include <a_agent>


/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of this agent.</summary>
\**--------------------------------------------------------------------------**/
main()
{
    // Send a welcome message
    chatprintf(COLOR_WHITE, "DOLL: Hi! I'm new here.");

    // Set agent properties
    SetModel("models/doll");
    SetSize(0.1);
    SetMaxForce(20);
    SetMaxSpeed(0.4);
    SetMass(0.02);

    // Just do nothing for now...
}
