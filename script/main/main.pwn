#include <string>
#include <a_main>
#include <a_drawable>

new isShiftDown = false;

new Drawable:text2D;
new Drawable:sphere;
/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of the simulation.</summary>
\**--------------------------------------------------------------------------**/
main()
{
    logprintf(COLOR_WHITE, "Testing123 123 test test test. Testing123 123 test test test. Testing123 123 test test test.Testing123 123 test test test.");

    chatprintf(COLOR_WHITE, "Some Chat Message Here.");

    //PlayAmbience("sounds/ambient", true, 0.015);
    SetBackgroundColor(0xff555555);
    CreatePlanes();
    CreateObjects();
    CreateGroundGraph();
    CreateRoads();
    CreateAgents();

    text2D = CreateDrawableText3D(5, 0, 5, COLOR_WHITE, "fonts/consolas", "Hello, World!");
    sphere = CreateDrawableLineShere(0, 0.5, 0, 0.5, COLOR_RED, COLOR_BLUE);
    ShowDrawable(sphere);
    ShowDrawable(text2D);
}

public OnKeyStateChanged(newKeys[], oldKeys[])
{
    isShiftDown = IsKeyDown(newKeys, KEY_LEFTSHIFT);


    if(IsKeyDown(newKeys, KEY_T) && !IsKeyDown(oldKeys, KEY_T))
    {
        SetDrawableColor(sphere, random(0xFFFFFFFF));
        SetDrawableColor2(sphere, random(0xFFFFFFFF));
    }

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

RandomTree(Float:x, Float:y)
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

    return AddGameObject("models/trees", 0.25, x, y, 0.2, 0.2, 0.2,
    0, frandom(-PI, PI), 0, -meshidx * 5, 0, 0, mesh);
}

CreateObjects()
{
    #define HOUSE_SIZE 0.45

    RandomTree(1,1);

    #define DEFAULT_SCALE 1, 1, 1
    AddGameObject("models/house02", HOUSE_SIZE, 3.9, 2.5, DEFAULT_SCALE, 0, DEG2RAD(-90));
    AddGameObject("models/house01", HOUSE_SIZE, 3.9, 3.8, 0.25,0.25,0.25, 0, DEG2RAD(-90));
    AddGameObject("models/house02", HOUSE_SIZE, 3.9, 4.5, DEFAULT_SCALE, 0, DEG2RAD(-90));
    AddGameObject("models/house02", HOUSE_SIZE, 3.9, 6.5, DEFAULT_SCALE, 0, DEG2RAD(-90));

    AddGameObject("models/house02", HOUSE_SIZE, 10.0, 0.0, DEFAULT_SCALE, 0, DEG2RAD(90));

    AddGameObject("models/house02", HOUSE_SIZE, -1, 3.8, DEFAULT_SCALE, 0, DEG2RAD(90));
    AddGameObject("models/house02", HOUSE_SIZE, -1, 2.0, DEFAULT_SCALE, 0, DEG2RAD(90));
    AddGameObject("models/house02", HOUSE_SIZE, -1, 0.0, DEFAULT_SCALE, 0, DEG2RAD(90));

    AddGameObject("models/house02", HOUSE_SIZE, 1.5, 3.8, DEFAULT_SCALE, 0, DEG2RAD(90));
    AddGameObject("models/house02", HOUSE_SIZE, 1.5, 2.0, DEFAULT_SCALE, 0, DEG2RAD(90));
    AddGameObject("models/supermarket", HOUSE_SIZE, 1.5, 0.0, 0.5, 0.5, 0.5, 0, DEG2RAD(90));
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
    AddAgent("car",0,0);
}
