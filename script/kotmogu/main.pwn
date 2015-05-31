#include <string>
#include <a_main>
#include <a_world>
#include <a_sound>

#include "main/carepackages"
#include "main/terrain"
#include "main/score"
#include "main/debug"

new orbs[4];

main()
{
    //state debug;

    chatprintf(COLOR_RED, "Started Temple of Kotmogu simulation.");

    //PlayAmbience("sounds/bftune", true, 0.005);
    SetBackgroundColor(0x00000000);

    ScoreInit();
    DebugInit();
    TerrainInit();

    CreateGroundGraph();
    CreateAgents();
}

public OnUpdate(Float:elapsed)
{
    ScoreUpdate();
    CarepackagesUpdate(elapsed);
}

public OnKeyStateChanged(newKeys[], oldKeys[])
{
    SetDrawGraphs(IsKeyDown(newKeys, KEY_F9));
}

CreateGroundGraph()
{
    // Create and fill a 'ground' graph and fill it automatically
    CreateGraph("ground");
    new nodes = FillGraph("ground", -2, -2, 6, 6, 0.3);

    logprintf(COLOR_MAGENTA, "Filled graph 'ground' with %d nodes.", nodes);
}

CreateAgents()
{
    orbs[0] = AddAgent("kotmogu/orb", -60, 0);
    orbs[1] = AddAgent("kotmogu/orb", 60, 0);
    orbs[2] = AddAgent("kotmogu/orb", 0, -60);
    orbs[3] = AddAgent("kotmogu/orb", 0, 60);

    /*AddAgent("kotmogu/tank", -100, -100, "team", 1);
    AddAgent("kotmogu/tank", -100, -95, "team", 1);
    AddAgent("kotmogu/tank", -100, -90, "team", 1);
    AddAgent("kotmogu/tank", -100, -85, "team", 1);
    AddAgent("kotmogu/tank", -100, -80, "team", 1);

    AddAgent("kotmogu/tank", 80, 100, "team", 2);
    AddAgent("kotmogu/tank", 85, 100, "team", 2);
    AddAgent("kotmogu/tank", 90, 100, "team", 2);
    AddAgent("kotmogu/tank", 95, 100, "team", 2);
    AddAgent("kotmogu/tank", 100, 100, "team", 2);*/

    AddAgent("kotmogu/tank", -70, -70, "team", 1);
    AddAgent("kotmogu/tank", -70, -65, "team", 1);
    AddAgent("kotmogu/tank", -70, -60, "team", 1);
    AddAgent("kotmogu/tank", -70, -55, "team", 1);
    AddAgent("kotmogu/tank", -70, -50, "team", 1);

    AddAgent("kotmogu/tank", 50, 70, "team", 2);
    AddAgent("kotmogu/tank", 55, 70, "team", 2);
    AddAgent("kotmogu/tank", 60, 70, "team", 2);
    AddAgent("kotmogu/tank", 65, 70, "team", 2);
    AddAgent("kotmogu/tank", 70, 70, "team", 2);
}
