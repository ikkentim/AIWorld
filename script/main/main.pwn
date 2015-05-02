#include <a_main>

/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of the simulation.</summary>
\**--------------------------------------------------------------------------**/
main()
{
    CreatePlanes();
    CreateObjects();
    CreateGroundGraph();
    CreateRoads();
    CreateAgents();
}

CreatePlanes()
{
    // Create the floor.
    for (new x = -5; x <= 5; x++)
        for (new y = -5; y <= 5; y++)
          AddQuadPlane(x*4, -0.01, y*4, 4, ROTATION_NONE, "textures/grass");
}

CreateObjects()
{
    // Add a few house models objects.
    AddGameObject("models/house02", 0.35, 3.9, 2.5, DEG2RAD(-90));
    AddGameObject("models/house02", 0.35, 3.9, 3.5, DEG2RAD(-90));
    AddGameObject("models/house02", 0.35, 3.9, 4.5, DEG2RAD(-90));
    AddGameObject("models/house02", 0.35, 3.9, 5.5, DEG2RAD(-90));
}

CreateGroundGraph()
{
    // Create and fill a 'ground' graph and fill it automatically
    CreateGraph("ground");
    FillGraph("ground", -5, -5, 5, 5, 1);
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
    for(new i=0;i<10;i++)
        AddAgent("doll", frandom(-5,5), frandom(-5,5));

    AddAgent("car", frandom(-5,5), frandom(-5,5));
}
