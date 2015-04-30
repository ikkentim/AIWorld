#include <a_agent>
#include <a_world>

main()
{
    SetModel("models/car");
    SetSize(0.3);
    SetMaxForce(15);
    SetMaxSpeed(1.2);
    SetMass(0.35);

    new
		Float:targetx,
        Float:targety,
        Float:targetnodex,
        Float:targetnodey,
        Float:startx,
        Float:starty;

    targetx = frandom(-5,5);
    targety = frandom(-5,5);

    GetPosition(startx,starty);
    GetClosestNode(startx,starty,startx,starty);

    GetClosestNode(targetx,targety,targetnodex,targetnodey);

    PushPathNode(targetx,targety);
    PushPath(startx,starty,targetnodex,targetnodey);

}

forward OnPathEnd();
public OnPathEnd()
{
    logprintf(COLOR_LIME, "OnPathEnd()");
}
