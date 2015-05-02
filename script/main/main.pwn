#include <a_main>

main()
{
    CreateGraph("road");

    CreateGraph("ground");
    FillGraph("ground", -10, -10, 10, 10, 0.2);
    
    for (new x = -5; x <= 5; x++)
        for (new y = -5; y <= 5; y++)
          AddQuadPlane(x*4, -0.01, y*4, 4, ROTATION_NONE, "textures/grass");

    AddGameObject("models/house02", 0.35, 3.9, 2.5, DEG2RAD(-90));
    AddGameObject("models/house02", 0.35, 3.9, 3.5, DEG2RAD(-90));
    AddGameObject("models/house02", 0.35, 3.9, 4.5, DEG2RAD(-90));
    AddGameObject("models/house02", 0.35, 3.9, 5.5, DEG2RAD(-90));

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

    for(new i=0;i<10;i++)
        AddAgent("doll", frandom(-5,5), frandom(-5,5));

    AddAgent("car", frandom(-5,5), frandom(-5,5));
}
