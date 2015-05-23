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

namespace AIWorld.Fuzzy.Terms
{
    public abstract class FuzzyTerm
    {
        public abstract float DOM { get; }
        public abstract void ClearDOM();
        public abstract void OrWithDOM(float value);
        public abstract void AddRule(FuzzyRule rule);

        public static implicit operator float(FuzzyTerm term)
        {
            if (term == null) throw new ArgumentNullException("term");
            return term.DOM;
        }

        public static FuzzyTerm operator &(FuzzyTerm left, FuzzyTerm right)
        {
            return new FuzzyAnd(left, right);
        }

        public static FuzzyTerm operator |(FuzzyTerm left, FuzzyTerm right)
        {
            return new FuzzyOr(left, right);
        }

        public FuzzyTerm Very()
        {
            return new FuzzyVery(this);
        }

        public FuzzyTerm Fairly()
        {
            return new FuzzyFairly(this);
        }

        public static bool operator true(FuzzyTerm term)
        {
            return term != null;
        }

        public static bool operator false(FuzzyTerm term)
        {
            return term == null;
        }
    }
}