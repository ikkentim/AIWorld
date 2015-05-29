#include <string>
#include <a_main>
#include <a_drawable>
#include <a_world>

new orbs[4];
new Drawable:alies_score;
new Drawable:axis_score;

new Drawable:zones[4*4];
/**--------------------------------------------------------------------------**\
<summary>Contains the setup logic of the simulation.</summary>
\**--------------------------------------------------------------------------**/
main()
{
    //state debug;

    chatprintf(COLOR_RED, "Started Temple of Kotmogu simulation.");

    SetGVar("score_alies", 0);
    SetGVar("score_axis", 0);

    alies_score = CreateDrawableText2D(5,5, COLOR_RED, "fonts/consolas", "Alies: 0");
    axis_score = CreateDrawableText2D(5,25, COLOR_BLUE, "fonts/consolas", "Axis: 0");
    ShowDrawable(alies_score);
    ShowDrawable(axis_score);

    //PlayAmbience("sounds/ambient", true, 0.015);
    SetBackgroundColor(0x00000000);
    CreatePlanes();
    CreateObjects();
    CreateGroundGraph();
    CreateAgents();

    #define UPLINE(%1,%2,%3) CreateDrawableLine(%1, 0, %2, %1, 1, %2, %3, %3)
    zones[0] = UPLINE(-10, -10, COLOR_PINK);
    zones[1] = UPLINE( 10, -10, COLOR_PINK);
    zones[2] = UPLINE(-10,  10, COLOR_PINK);
    zones[3] = UPLINE( 10,  10, COLOR_PINK);

    zones[4] = UPLINE(-30, -30, COLOR_PINK);
    zones[5] = UPLINE( 30, -30, COLOR_PINK);
    zones[6] = UPLINE(-30,  30, COLOR_PINK);
    zones[7] = UPLINE( 30,  30, COLOR_PINK);

    zones[8] = UPLINE(-50, -50, COLOR_PINK);
    zones[9] = UPLINE( 50, -50, COLOR_PINK);
    zones[10] = UPLINE(-50,  50, COLOR_PINK);
    zones[11] = UPLINE( 50,  50, COLOR_PINK);

    zones[12] = UPLINE(-70, -70, COLOR_PINK);
    zones[13] = UPLINE( 70, -70, COLOR_PINK);
    zones[14] = UPLINE(-70,  70, COLOR_PINK);
    zones[15] = UPLINE( 70,  70, COLOR_PINK);

    for(new i=0;i<16;i++)
        ShowDrawable(zones[i]);
}

public OnUpdate(Float:elapsed)
{
    new alies[32], axis[32];
    strformat(alies, sizeof(alies), false, "Alies: %d", GetGVar("score_alies"));
    strformat(axis, sizeof(axis), false, "Axis:  %d", GetGVar("score_axis"));

    SetDrawableText(alies_score, alies);
    SetDrawableText(axis_score, axis);
}

public OnKeyStateChanged(newKeys[], oldKeys[])
{
    SetDrawGraphs(IsKeyDown(newKeys, KEY_F9));
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

    /*CreateHouse02(3.9, 2.5, DEG2RAD(-90));
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

    CreateSupermarket(1.5, 0.0, DEG2RAD(90));*/
}

CreatePlanes()
{
    // Create the floor.
    for (new x = -10; x <= 10; x++)
        for (new y = -10; y <= 10; y++)
          AddQuadPlane(x*20, -0.01, y*20, 20, ROTATION_NONE, "textures/grass");
}

CreateGroundGraph()
{
    // Create and fill a 'ground' graph and fill it automatically
    CreateGraph("ground");
    new nodes = FillGraph("ground", -2, -2, 6, 6, 0.3);

    logprintf(COLOR_MAGENTA, "Filled graph 'ground' with %d nodes.", nodes);
}

CreateAgents()
{
    AddGameObject("models/ammocrate", 1, 7, 7, 0.05, 0.05, 0.05);

    orbs[0] = AddAgent("kotmogu/orb", -60, 0);
    orbs[1] = AddAgent("kotmogu/orb", 60, 0);
    orbs[2] = AddAgent("kotmogu/orb", 0, -60);
    orbs[3] = AddAgent("kotmogu/orb", 0, 60);

    /*AddAgent("kotmogu/tank", -100, -100, "team", 1);
    AddAgent("kotmogu/tank", -100, -95, "team", 1);
    AddAgent("kotmogu/tank", -100, -90, "team", 1);
    AddAgent("kotmogu/tank", -100, -85, "team", 1);
    AddAgent("kotmogu/tank", -100, -80, "team", 1);

    AddAgent("kotmogu/tank", 80, 100, "team", 2);
    AddAgent("kotmogu/tank", 85, 100, "team", 2);
    AddAgent("kotmogu/tank", 90, 100, "team", 2);
    AddAgent("kotmogu/tank", 95, 100, "team", 2);
    AddAgent("kotmogu/tank", 100, 100, "team", 2);*/

    AddAgent("kotmogu/tank", -70, -70, "team", 1);
    AddAgent("kotmogu/tank", -70, -65, "team", 1);
    AddAgent("kotmogu/tank", -70, -60, "team", 1);
    AddAgent("kotmogu/tank", -70, -55, "team", 1);
    AddAgent("kotmogu/tank", -70, -50, "team", 1);

    AddAgent("kotmogu/tank", 50, 70, "team", 2);
    AddAgent("kotmogu/tank", 55, 70, "team", 2);
    AddAgent("kotmogu/tank", 60, 70, "team", 2);
    AddAgent("kotmogu/tank", 65, 70, "team", 2);
    AddAgent("kotmogu/tank", 70, 70, "team", 2);
}
