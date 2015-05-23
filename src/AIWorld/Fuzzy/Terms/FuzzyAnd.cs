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

namespace FuzzyLogic.Fuzzy.Terms
{
    public class FuzzyAnd : FuzzyTerm
    {
        private readonly FuzzyTerm _left;
        private readonly FuzzyTerm _right;

        public FuzzyAnd(FuzzyTerm left, FuzzyTerm right)
        {
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");
            _left = left;
            _right = right;
        }

        #region Implementation of FuzzyTerm

        public override float DOM
        {
            get { return Math.Min(_left, _right); }
        }

        public override void ClearDOM()
        {
            _left.ClearDOM();
            _right.ClearDOM();
        }

        public override void OrWithDOM(float value)
        {
            _left.OrWithDOM(value);
            _right.OrWithDOM(value);
        }

        public override void AddRule(FuzzyRule rule)
        {
            _left.AddRule(rule);
            _right.AddRule(rule);
        }

        #endregion
    }
}