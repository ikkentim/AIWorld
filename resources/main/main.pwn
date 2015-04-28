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
	-2.0,	8.0,
	-2.0,	7.0,
	-2.0,	6.0,
	-2.0,	5.0,
	-2.0,	4.0,
	-2.0,	3.0,
	-3.0,	2.0,
	-4.0,	2.0,
	-5.0,	2.0,
	-6.0,	2.0,
	-7.0,	2.0
	];
	AddRoad(road1, 11);
	
	AddAgent("car", -1, -1);
}
