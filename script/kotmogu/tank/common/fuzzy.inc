#include <a_fuzzy>
#include "../../common/carepackages"

new static
    Fuzzy:health,
    Fuzzy:area_power,
    Fuzzy:ammo,
    Fuzzy:carepackage,
    Fuzzy:orb,
    Fuzzy:have_orb,
    Fuzzy:orbs,
    Fuzzy:enemy_orbs,

    Fuzzy:orb_desirability,
    Fuzzy:powerup_desirability,
    Fuzzy:carepackage_desirability,
    Fuzzy:defend_desirability,
    Fuzzy:atack_desirability,
    Fuzzy:hold_desirability;

stock AddDesirabilitySet(Fuzzy:variable)
{
    AddFuzzyLeftShoulder(variable, "undesirable", 0, 25, 50);
    AddFuzzyTriangle(variable, "desirable", 25, 50, 75);
    AddFuzzyRightShoulder(variable, "very_desirable", 50, 75, 100);
}

stock LoadFuzzy()
{
    health = CreateFuzzyVariable("health");
    ammo = CreateFuzzyVariable("ammo");
    area_power = CreateFuzzyVariable("area_power");
    carepackage = CreateFuzzyVariable("carepackage");
    orb = CreateFuzzyVariable("orb");
    have_orb = CreateFuzzyVariable("have_orb");
    orbs = CreateFuzzyVariable("orbs");
    enemy_orbs = CreateFuzzyVariable("enemy_orbs");

    orb_desirability = CreateFuzzyVariable("orb_desirability");
    powerup_desirability = CreateFuzzyVariable("powerup_desirability");
    carepackage_desirability = CreateFuzzyVariable("carepackage_desirability");
    defend_desirability = CreateFuzzyVariable("defend_desirability");
    atack_desirability = CreateFuzzyVariable("atack_desirability");
    hold_desirability = CreateFuzzyVariable("hold_desirability");

    AddFuzzyLeftShoulder(health, "low", 0, 30, 50);
    AddFuzzyTriangle(health, "medium", 30, 50, 70);
    AddFuzzyRightShoulder(health, "high", 50, 70, 100);

    AddFuzzyLeftShoulder(ammo, "low", 0, 5, 15);
    AddFuzzyTrapezium(ammo, "medium", 5, 15, 25, 35);
    AddFuzzyRightShoulder(ammo, "high", 25, 35, 10000);

    AddFuzzyLeftShoulder(area_power, "low", 0, 1, 2);
    AddFuzzyTriangle(area_power, "medium", 1, 2, 3);
    AddFuzzyRightShoulder(area_power, "high", 2, 3, 4);

    AddFuzzyLeftShoulder(carepackage, "close", 0, 15, 30);
    AddFuzzyTriangle(carepackage, "medium", 15, 30, 45);
    AddFuzzyRightShoulder(carepackage, "far", 30, 45, 10000);

    AddFuzzyLeftShoulder(orb, "close", 0, 30, 45);
    AddFuzzyTrapezium(orb, "medium", 30, 45, 75, 90);
    AddFuzzyTrapezium(orb, "far", 75, 90, 110, 125);
    AddFuzzyRightShoulder(orb, "no", 110, 125, 10000);

    AddFuzzyLeftShoulder(have_orb, "doesnt", 0, 0, 1);
    AddFuzzyRightShoulder(have_orb, "does", 0, 1, 1);

    AddFuzzyLeftShoulder(orbs, "few", 0, 1, 3);
    AddFuzzyRightShoulder(orbs, "many", 1, 3, 4);

    AddFuzzyLeftShoulder(enemy_orbs, "few", 0, 1, 3);
    AddFuzzyRightShoulder(enemy_orbs, "many", 1, 3, 4);

    AddDesirabilitySet(orb_desirability);
    AddDesirabilitySet(powerup_desirability);
    AddDesirabilitySet(carepackage_desirability);
    AddDesirabilitySet(defend_desirability);
    AddDesirabilitySet(atack_desirability);
    AddDesirabilitySet(hold_desirability);

    // orb_desirability
    AddFuzzyRule("IF close orb AND doesnt have_orb THEN very very_desirable orb_desirability");
    AddFuzzyRule("IF medium orb AND doesnt have_orb THEN very_desirable orb_desirability");
    AddFuzzyRule("IF low ammo OR low health AND doesnt have_orb THEN undesirable orb_desirability");
    AddFuzzyRule("IF far orb AND high health AND high ammo AND doesnt have_orb THEN very very_desirable orb_desirability");
    AddFuzzyRule("IF far orb AND very few orbs AND doesnt have_orb THEN very_desirable orb_desirability");
    AddFuzzyRule("IF no orb THEN undesirable orb_desirability");

    // powerup_desirability
    AddFuzzyRule("IF medium area_power AND does have_orb THEN desirable powerup_desirability");
    AddFuzzyRule("IF low area_power AND does have_orb THEN very_desirable powerup_desirability");
    /*AddFuzzyRule("IF doesnt have_orb THEN undesirable powerup_desirability");*/

    // carepackage_desirability
    AddFuzzyRule("IF low health AND far carepackage THEN desirable carepackage_desirability");
    AddFuzzyRule("IF medium health AND far carepackage THEN undesirable carepackage_desirability");
    AddFuzzyRule("IF high health AND far carepackage THEN undesirable carepackage_desirability");
    AddFuzzyRule("IF low health AND medium carepackage THEN very_desirable carepackage_desirability");
    AddFuzzyRule("IF medium health AND medium carepackage THEN undesirable carepackage_desirability");
    AddFuzzyRule("IF high health AND medium carepackage THEN undesirable carepackage_desirability");
    AddFuzzyRule("IF low health AND close carepackage THEN very_desirable carepackage_desirability");
    AddFuzzyRule("IF medium health AND close carepackage THEN desirable carepackage_desirability");
    AddFuzzyRule("IF high health AND close carepackage THEN undesirable carepackage_desirability");

    AddFuzzyRule("IF low ammo AND far carepackage THEN desirable carepackage_desirability");
    AddFuzzyRule("IF medium ammo AND far carepackage THEN undesirable carepackage_desirability");
    AddFuzzyRule("IF high ammo AND far carepackage THEN undesirable carepackage_desirability");
    AddFuzzyRule("IF low ammo AND medium carepackage THEN very_desirable carepackage_desirability");
    AddFuzzyRule("IF medium ammo AND medium carepackage THEN undesirable carepackage_desirability");
    AddFuzzyRule("IF high ammo AND medium carepackage THEN undesirable carepackage_desirability");
    AddFuzzyRule("IF low ammo AND close carepackage THEN very_desirable carepackage_desirability");
    AddFuzzyRule("IF medium ammo AND close carepackage THEN desirable carepackage_desirability");
    AddFuzzyRule("IF high ammo AND close carepackage THEN undesirable carepackage_desirability");

    // TODO: Add some friendly_orb_holder_near, enemy_orb_holder_near variable
    // defend_desirability
    AddFuzzyRule("IF many orbs AND doesnt have_orb THEN very_desirable defend_desirability");
    AddFuzzyRule("IF few orbs AND doesnt have_orb THEN desirable defend_desirability");
    AddFuzzyRule("IF very few orbs AND doesnt have_orb THEN undesirable defend_desirability");

    // atack_desirability
    AddFuzzyRule("IF many enemy_orbs AND doesnt have_orb THEN very_desirable atack_desirability");
    AddFuzzyRule("IF few enemy_orbs AND doesnt have_orb THEN desirable atack_desirability");
    AddFuzzyRule("IF very few enemy_orbs AND doesnt have_orb THEN undesirable atack_desirability");

    // hold_desirability
    AddFuzzyRule("IF high area_power AND does have_orb THEN desirable hold_desirability");
}


stock SetFuzzyVariables()
{
    new Float:x, Float:y, Float:ax, Float:ay,
        Float:carepackage_distance=1000000,
        Float:orb_distance=1000000;

    GetPosition(x, y);


    new crateid = GetNearestPackage(x, y);
    if(crateid >= 0)
    {
        GetEntityPosition(crateid, ax, ay);
        carepackage_distance = floatsqroot(floatabs(x-ax) * floatabs(x-ax) +
        floatabs(y-ay) * floatabs(y-ay));
    }

    new orbid = FindNearestAgentByVar(x, y, 1000, "team", 0, "kotmogu/orb");
    if(orbid >= 0)
    {
        GetEntityPosition(orbid, ax, ay);
        orb_distance = floatsqroot(floatabs(x-ax) * floatabs(x-ax) +
        floatabs(y-ay) * floatabs(y-ay));
    }

    Fuzzify(health, GetVarFloat("health"));
    Fuzzify(ammo, float(GetVar("ammo")));

    Fuzzify(Fuzzy:area_power, float(GetAreaPower(x, y)));

    Fuzzify(carepackage, carepackage_distance);
    Fuzzify(orb, orb_distance);
    Fuzzify(have_orb, GetVar("orb") == -1 ? 0 : 1);
    Fuzzify(orbs, float(GetVar("team") == 1
    ? GetGVar("orbs_axis")
    : GetGVar("orbs_alies")));
    Fuzzify(enemy_orbs, float(GetVar("team") == 2
    ? GetGVar("orbs_axis")
    : GetGVar("orbs_alies")));
}

stock Float:GetOrbDesirability()
    return Defuzzify(orb_desirability);

stock Float:GetPowerupDesirability()
    return Defuzzify(powerup_desirability);

stock Float:GetCarepackageDesirability()
    return Defuzzify(carepackage_desirability);

stock Float:GetDefendDesirability()
    return Defuzzify(defend_desirability);

stock Float:GetAtackDesirability()
    return Defuzzify(atack_desirability);

stock Float:GetHoldDesirability()
    return Defuzzify(hold_desirability);
