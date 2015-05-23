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
using System.Collections.Generic;
using System.Diagnostics;
using AIWorld.Fuzzy.Sets;

namespace AIWorld.Fuzzy
{
    public class FuzzyVariable
    {
        private readonly Dictionary<string, FuzzySet> _sets = new Dictionary<string, FuzzySet>();
        private List<FuzzyRule> _rules = new List<FuzzyRule>();
        private float _maxRange;
        private float _minRange;

        public FuzzySet this[string name]
        {
            get { return _sets[name]; }
        }

        public FuzzySet Add(string name, FuzzySet set)
        {
            _sets[name] = set;
            AdjustRangeToFit(set.Min, set.Max);

            set.Variable = this;
            return set;
        }

        public void Add(FuzzyRule rule)
        {
            if (rule == null) throw new ArgumentNullException("rule");
            _rules.Add(rule);
        }

        public void Fuzzify(float value)
        {
            if (value < _minRange || value > _maxRange)
                throw new ArgumentOutOfRangeException("value", value, "out of range.");

            foreach (var set in _sets)
            {
                set.Value.SetDOM(set.Value.CalculateDOM(value));
            }
        }

        public float Defuzzify()
        {
            foreach (var rule in _rules)
                rule.SetConfidenceOfConsequentToZero();

            foreach (var rule in _rules)
                rule.Calculate();

            //MaxAv
            float bottom = 0f;
            float top = 0f;

            foreach (var set in _sets)
            {
                Debug.WriteLine("{0} member of {1}", set.Value.DOM, set.Key);
                bottom += set.Value.DOM;
                top += set.Value.RepresentativeValue*set.Value.DOM;
            }

            if (bottom == 0) return 0;

            return top/bottom;
        }

        private void AdjustRangeToFit(float min, float max)
        {
            _minRange = Math.Min(_minRange, min);
            _maxRange = Math.Max(_maxRange, max);
        }
    }
}