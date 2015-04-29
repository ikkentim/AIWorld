#if defined _agent_included
  #endinput
#endif
#define _agent_included

/**************************************************/
/* Sets the model of the agent */
native SetModel(model[]);

/**************************************************/
/* Sets the size of the agent */
native SetSize(Float:size);

/**************************************************/
/* Sets the maximum force of the agent */
native SetMaxForce(Float:force);

/**************************************************/
/* Sets the maximum speed of the agent */
native SetMaxSpeed(Float:speed);

/**************************************************/
/* Sets the mass of the agent */
native SetMass(Float:mass);