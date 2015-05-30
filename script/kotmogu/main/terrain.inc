#include <a_drawable>

TerrainInit()
{
    CreatePlanes();
    CreateObjects();
}


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

stock CreateRandomTree(Float:x, Float:y)
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

stock CreateHouse01(Float:x, Float:y, Float:angle)
{
    CreateCollisionSphere(x, y, SIZE_HOUSE01);
    AddGameObject("models/house01", SIZE_HOUSE01, x, y, 0.25,0.25,0.25,
        0, angle, 0);
}

stock CreateHouse02(Float:x, Float:y, Float:angle)
{
    CreateCollisionSphere(x, y, SIZE_HOUSE02);
    AddGameObject("models/house02", SIZE_HOUSE02, x, y, 1, 1, 1,
        0, angle, 0);
}

stock CreateSupermarket(Float:x, Float:y, Float:angle)
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
