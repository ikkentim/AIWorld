#if defined _fuzzy_included
    #endinput
#endif
#define _fuzzy_included

native FuzzyVar:CreateFuzzyVariable(const name[]);
native IsValidFuzzyVariable(FuzzyVar:variable);
native DeleteFuzzyVariable(FuzzyVar:variable);
native AddFuzzyRule(const rule[]);
native Fuzzify(FuzzyVar:variable, Float:value);
native Float:Defuzzify(FuzzyVar:variable);
native AddFuzzyLeftShoulder(FuzzyVar:variable, const name[], Float:left,
    Float:middle, Float:right);
native AddFuzzyRightShoulder(FuzzyVar:variable, const name[], Float:left,
    Float:middle, Float:right);
native AddFuzzyTrapezium(FuzzyVar:variable, const name[], Float:left,
    Float:leftmiddle, Float:rightmiddle, Float:right);
native AddFuzzyTriangle(FuzzyVar:variable, const name[], Float:left,
    Float:middle, Float:right);
