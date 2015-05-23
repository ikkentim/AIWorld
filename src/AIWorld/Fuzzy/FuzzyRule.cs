// FuzzyLogic
// Copyright 2015 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using AIWorld.Fuzzy.Terms;

namespace AIWorld.Fuzzy
{
    public class FuzzyRule
    {
        private readonly FuzzyTerm _antecent;
        private readonly FuzzyTerm _consequence;

        private FuzzyRule(FuzzyTerm antecent, FuzzyTerm consequence)
        {
            _antecent = antecent;
            _consequence = consequence;
        }

        public void SetConfidenceOfConsequentToZero()
        {
            _consequence.ClearDOM();
        }

        public void Calculate()
        {
            _consequence.OrWithDOM(_antecent);
        }

        public static void Create(FuzzyTerm antecent, FuzzyTerm consequence)
        {
            if (antecent == null) throw new ArgumentNullException("antecent");
            if (consequence == null) throw new ArgumentNullException("consequence");
            consequence.AddRule(new FuzzyRule(antecent, consequence));
        }
    }
}