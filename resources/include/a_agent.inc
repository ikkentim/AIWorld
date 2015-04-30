#if defined _agent_included
    #endinput
#endif
#define _agent_included

native GetPosition(&Float:x, &Float:y);
native ClearPathStack();
native PopPathNode(&Float:x, &Float:y);
native PushPathNode(Float:x, Float:y);
native PeekPathNode(&Float:x, &Float:y);
native PushPath(Float:xstart, Float:ystart, Float:xend, Float:yend);

native SetModel(const model[]);

native SetMass(Float:mass);
native Float:GetMass();
native SetMaxSpeed(Float:speed);
native Float:GetMaxSpeed();
native SetMaxForce(Float:force);
native Float:GetMaxForce();
native SetSize(Float:size);
native Float:GetSize();
