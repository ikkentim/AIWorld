#if defined _main_included
  #endinput
#endif
#define _main_included

/**************************************************/
/* Adds a game object to the world */
native AddGameObject(modelname[], Float:size, Float:x, Float:y, Float:angle=0.0);

/**************************************************/
/* Adds a road to the world */
native AddRoad(Float:pointsxy[], count = sizeof pointsxy);

/**************************************************/
/* Adds an agent to the world */
native AddAgent(scriptname[], Float:x, Float:y);