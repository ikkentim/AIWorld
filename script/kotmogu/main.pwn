#include <string>
#include <a_main>
#include <a_world>
#include <math>

#include "main/carepackages"
#include "main/debug"
#include "main/score"
#include "main/terrain"

main()
{
    chatprintf(COLOR_RED, "Started Temple of Kotmogu simulation.");

    // To enable debug information, uncomment the following line:
    /*state debug;*/

    // Set the scene
    //PlayAmbience("sounds/bftune", true, 0.02);
    SetBackgroundColor(0x00000000);

    ScoreInit();
    DebugInit();


    CreateAgents();

    TerrainInit();

    CreateGroundGraph();
}

public OnUpdate(Float:elapsed)
{
    ScoreUpdate();
    CarepackagesUpdate(elapsed);
}

public OnKeyStateChanged(newKeys[], oldKeys[])
{
    // Draw graphs while F9 is pressed.
    SetDrawGraphs(IsKeyDown(newKeys, KEY_F9));
}
public OnMouseClick(button, Float:x, Float:y)
{
    if(button == 1)
        CallPublicFunction("OnTankSelected", "d", -1);
}
CreateGroundGraph()
{
    // Create and fill a 'ground' graph and fill it automatically.
    CreateGraph("ground");
    new tmp,
        start,
        nodes;
    start = timestamp(tmp, tmp, tmp,tmp, tmp, tmp, tmp);
    nodes = FillGraph("ground", -100, -100, 100, 100, 2);

    logprintf(COLOR_MAGENTA,
        "Filled graph 'ground' with %d nodes in %f seconds.", nodes,
        float(timestamp(tmp, tmp, tmp,tmp, tmp, tmp, tmp) - start) / 1000);
}

CreateAgents()
{
    // Spawn the orb agents.
    AddAgent("kotmogu/orb", -60, 0);
    AddAgent("kotmogu/orb", 60, 0);
    AddAgent("kotmogu/orb", 0, -60);
    AddAgent("kotmogu/orb", 0, 60);

    // Spawn team 1.
    AddAgent("kotmogu/tank", -70, -70, "team", 1);
    AddAgent("kotmogu/tank", -70, -65, "team", 1);
    AddAgent("kotmogu/tank", -70, -60, "team", 1);
    AddAgent("kotmogu/tank", -70, -55, "team", 1);
    AddAgent("kotmogu/tank", -70, -50, "team", 1);

    // Spawn team 2.
    AddAgent("kotmogu/tank", 50, 70, "team", 2);
    AddAgent("kotmogu/tank", 55, 70, "team", 2);
    AddAgent("kotmogu/tank", 60, 70, "team", 2);
    AddAgent("kotmogu/tank", 65, 70, "team", 2);
    AddAgent("kotmogu/tank", 70, 70, "team", 2);
}
