#if defined _fuzzy_included
    #endinput
#endif
#define _fuzzy_included

native Fuzzy:CreateFuzzyVariable(const name[]);
native IsValidFuzzyVariable(Fuzzy:variable);
native DeleteFuzzyVariable(Fuzzy:variable);
native AddFuzzyRule(const rule[]);
native Fuzzify(Fuzzy:variable, Float:value);
native Float:Defuzzify(Fuzzy:variable);
native AddFuzzyLeftShoulder(Fuzzy:variable, const name[], Float:left,
    Float:middle, Float:right);
native AddFuzzyRightShoulder(Fuzzy:variable, const name[], Float:left,
    Float:middle, Float:right);
native AddFuzzyTrapezium(Fuzzy:variable, const name[], Float:left,
    Float:leftmiddle, Float:rightmiddle, Float:right);
native AddFuzzyTriangle(Fuzzy:variable, const name[], Float:left, Float:middle,
    Float:right);
