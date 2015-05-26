#include <string>
#include <a_main>
#include <a_drawable>

new isShiftDown = false;

/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of the simulation.</summary>
\**--------------------------------------------------------------------------**/
main()
{
    //state debug;

    //PlayAmbience("sounds/ambient", true, 0.015);
    SetBackgroundColor(0x00000000);
    CreatePlanes();
    CreateObjects();
    CreateGroundGraph();
    CreateRoads();
    CreateAgents();
}

public OnKeyStateChanged(newKeys[], oldKeys[])
{
    isShiftDown = IsKeyDown(newKeys, KEY_LEFTSHIFT);

    if(IsKeyDown(newKeys, KEY_A) && !IsKeyDown(oldKeys, KEY_A))
    {
        logprintf(COLOR_WHITE, "Spawned an agent!");
        AddAgent("doll", frandom(-5,5), frandom(-5,5));
    }

    SetDrawGraphs(IsKeyDown(newKeys, KEY_F9));
}

public OnMouseClick(button, Float:x, Float:y)
{
    if(button == 1 && isShiftDown)
        SetTarget(x,y);
}

 /*============================*
 *      OBJECT PLACEMENT      *
*============================*/

#define SIZE_TREE           (0.25)
#define SIZE_SUPERMARKET    (0.70)
#define SIZE_HOUSE01        (0.45)
#define SIZE_HOUSE02        (0.45)

stock CreateCollisionSphere(Float:x, Float:y, Float:radius) <debug>
{
    ShowDrawable(
        CreateDrawableLineCylinder(x, 0, y, 0, 1, 0, 1.0, radius,
            COLOR_BLACK, COLOR_BLACK));
}

stock CreateCollisionSphere(Float:x, Float:y, Float:radius) <>
{
    #pragma unused x
    #pragma unused y
    #pragma unused radius
}

CreateRandomTree(Float:x, Float:y)
{
    new mesh[10];
    new meshidx = random(7);

    switch(meshidx)
    {
        case 0: strcopy(mesh, "tree");
        case 1: strcopy(mesh, "tree.001");
        case 2: strcopy(mesh, "tree.003");
        case 3: strcopy(mesh, "tree.008");
        case 4: strcopy(mesh, "tree.004");
        case 5: strcopy(mesh, "tree.006");
        case 6: strcopy(mesh, "tree.007");
    }

    CreateCollisionSphere(x, y, SIZE_TREE);
    AddGameObject("models/trees", SIZE_TREE, x, y, 0.2, 0.2, 0.2,
        0, frandom(-PI, PI), 0, -meshidx * 5, 0, 0, mesh);
}

CreateHouse01(Float:x, Float:y, Float:angle)
{
    CreateCollisionSphere(x, y, SIZE_HOUSE01);
    AddGameObject("models/house01", SIZE_HOUSE01, x, y, 0.25,0.25,0.25,
        0, angle, 0);
}

CreateHouse02(Float:x, Float:y, Float:angle)
{
    CreateCollisionSphere(x, y, SIZE_HOUSE02);
    AddGameObject("models/house02", SIZE_HOUSE02, x, y, 1, 1, 1,
        0, angle, 0);
}

CreateSupermarket(Float:x, Float:y, Float:angle)
{
    CreateCollisionSphere(x, y, SIZE_SUPERMARKET);
    AddGameObject("models/supermarket", SIZE_SUPERMARKET, x, y, 0.5, 0.5, 0.5,
        0, angle, 0, 0, 0, 0.8);
}

CreateObjects()
{
    CreateRandomTree(1,1);

    CreateHouse02(3.9, 2.5, DEG2RAD(-90));
    CreateHouse01(4.5, 8.5, DEG2RAD(90));

    CreateHouse02(3.9, 3.8, DEG2RAD(-90));
    CreateHouse02(3.9, 4.5, DEG2RAD(-90));
    CreateHouse02(3.9, 6.5, DEG2RAD(-90));
    CreateHouse02(10.0, 0.0, DEG2RAD(90));

    CreateHouse02(-1, 3.8, DEG2RAD(90));
    CreateHouse02(-1, 2.0, DEG2RAD(90));
    CreateHouse02(-1, 0.0, DEG2RAD(90));

    CreateHouse02(1.5, 3.8, DEG2RAD(90));
    CreateHouse02(1.5, 2.0, DEG2RAD(90));

    CreateSupermarket(1.5, 0.0, DEG2RAD(90));
}

CreatePlanes()
{
    // Create the floor.
    for (new x = -5; x <= 5; x++)
        for (new y = -5; y <= 5; y++)
          AddQuadPlane(x*4, -0.01, y*4, 4, ROTATION_NONE, "textures/grass");
}

CreateGroundGraph()
{
    // Create and fill a 'ground' graph and fill it automatically
    CreateGraph("ground");
    new nodes = FillGraph("ground", -2, -2, 6, 6, 0.3);

    logprintf(COLOR_MAGENTA, "Filled graph 'ground' with %d nodes.", nodes);
}

CreateRoads()
{
    // Create a 'road' graph and fill it with a few roads.
    CreateGraph("road");

    new Float:road1[] = [
        -1.0, 9.0,
        -2.0,  8.0,
        -2.0,  7.0,
        -2.0,  6.0,
        -2.0,  5.0,
        -2.0,  4.0,
        -2.0,  3.0,
        -3.0,  2.0,
        -4.0,  2.0,
        -5.0,  2.0,
        -6.0,  2.0,
        -7.0,  2.0
    ];

    new Float:road2[] = [
        0.0, 0.0,
        1.0, 0.0,
        2.0,  0.0,
        3.0,  1.0,
        3.0,  2.0,
        3.0,  3.0,
        3.0,  4.0,
        3.0,  5.0,
        3.0,  6.0,
        3.0,  7.0,
        3.0,  8.0,
        2.0,  9.0,
        1.0,  9.0,
        0.0,  9.0
    ];

    AddRoad("road", road1);
    AddRoad("road", road2);
}

CreateAgents()
{
    //AddAgent("car", frandom(-5,5), frandom(-5,5));
    AddAgent("tank", 3, 10, "tx,ty", 10.0, 12.0);
    AddAgent("tank", 10, 12, "tx,ty", 3.0, 10.0);
}
