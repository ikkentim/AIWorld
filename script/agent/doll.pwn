#include <a_agent>
#include <a_world>

main()
{
    SetModel("models/doll");
    SetSize(0.1);
    SetMaxForce(20);
    SetMaxSpeed(0.4);
    SetMass(0.02);

    new Float:targetx,
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
    logprintf(COLOR_LIME, "[doll] OnPathEnd()");
}
