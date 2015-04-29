#include <float>
#include <math>
#include <a_main>
main()
{
  AddGameObject("models/house02", 0.35, 3.9, 2.5, DEG2RAD(-90));
  AddGameObject("models/house02", 0.35, 3.9, 3.5, DEG2RAD(-90));
  AddGameObject("models/house02", 0.35, 3.9, 4.5, DEG2RAD(-90));
  AddGameObject("models/house02", 0.35, 3.9, 5.5, DEG2RAD(-90));

  new Float:road1[] = [
  //-1.0, 9.0,
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

  AddRoad(road1);
  AddRoad(road2);

	for(new i=0;i<15;i++)
  AddAgent("car", frandom(-5,5), frandom(-5,5));
}
