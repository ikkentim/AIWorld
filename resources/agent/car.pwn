#include <float>
#include <a_agent>
#include <a_world>
#include <common>

main()
{
	SetModel("models/car");
	SetSize(0.2);
	SetMaxForce(15);
	SetMaxSpeed(1.2);
	SetMass(0.35);
	
	new Float:x,Float:y;
	GetClosestNode(10,10,x,y);
	log("Car spawned");
	logf("Debug: (%f, %f)", x, y);

}

forward OnPathEnd();
public OnPathEnd()
{
	log("OnPathEnd()");
}
