#if defined _main_included
  #endinput
#endif
#define _main_included

native AddGameObject(modelname[], Float:size, Float:x, Float:y, Float:angle=0.0);
native AddRoad(Float:pointsxy[], count = sizeof pointsxy);
native AddAgent(scriptname[], Float:x, Float:y);
