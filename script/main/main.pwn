#include <a_main>

new isShiftDown = false;

native PlayAmbience(const sound[], isLooped=false, Float:volume=1.0,
    Float:pitch=0.0, Float:pan=0.0);

/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of the simulation.</summary>
\**--------------------------------------------------------------------------**/
main()
{
    logprintf(COLOR_WHITE, "Testing123 123 test test test. Testing123 123 test test test. Testing123 123 test test test.Testing123 123 test test test.");

    chatprintf(COLOR_WHITE, "Some Chat Message Here.");

    PlayAmbience("sounds/ambient", true, 0.015);
    SetBackgroundColor(0xff555555);
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

    if(IsKeyDown(newKeys, KEY_F9) != GetDrawGraphs())
        SetDrawGraphs(IsKeyDown(newKeys, KEY_F9));
}

public OnMouseClick(button, Float:x, Float:y)
{
    if(button == 1 && isShiftDown)
        SetTarget(x,y);

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
    /*AddGameObject("models/house02", 0.35, 3.9, 2.5, DEG2RAD(-90));
    AddGameObject("models/house02", 0.35, 3.9, 3.5, DEG2RAD(-90));
    AddGameObject("models/house02", 0.35, 3.9, 4.5, DEG2RAD(-90));
    AddGameObject("models/house02", 0.35, 3.9, 5.5, DEG2RAD(-90));*/

    #define HOUSE_SIZE 0.5

    AddGameObject("models/house02", HOUSE_SIZE, 3.9, 2.5, DEG2RAD(-90));
    AddGameObject("models/house02", HOUSE_SIZE, 3.9, 3.8, DEG2RAD(-90));
    AddGameObject("models/house02", HOUSE_SIZE, 3.9, 4.5, DEG2RAD(-90));
    AddGameObject("models/house02", HOUSE_SIZE, 3.9, 6.5, DEG2RAD(-90));

    AddGameObject("models/house02", HOUSE_SIZE, 10.0, 10.0, DEG2RAD(90));

    AddGameObject("models/house02", HOUSE_SIZE, -1, 3.8, DEG2RAD(90));
    AddGameObject("models/house02", HOUSE_SIZE, -1, 2.0, DEG2RAD(90));
    AddGameObject("models/house02", HOUSE_SIZE, -1, 0.0, DEG2RAD(90));

    AddGameObject("models/house02", HOUSE_SIZE, 1.5, 3.8, DEG2RAD(90));
    AddGameObject("models/house02", HOUSE_SIZE, 1.5, 2.0, DEG2RAD(90));
    AddGameObject("models/house02", HOUSE_SIZE, 1.5, 0.0, DEG2RAD(90));
}

CreateGroundGraph()
{
    // Create and fill a 'ground' graph and fill it automatically
    CreateGraph("ground");
    new nodes = FillGraph("ground", -2, -2, 6, 6, 0.5);

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
    SetTargetEntity(AddAgent("car",0,0, "test:f", 123.1237));
}
