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
    public class FuzzyOr : FuzzyTerm
    {
        private readonly FuzzyTerm _left;
        private readonly FuzzyTerm _right;

        public FuzzyOr(FuzzyTerm left, FuzzyTerm right)
        {
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");
            _left = left;
            _right = right;
        }

        #region Implementation of FuzzyTerm

        public override float DOM
        {
            get { return Math.Max(_left, _right); }
        }

        public override void ClearDOM()
        {
            throw new NotImplementedException("Invalid context");
        }

        public override void OrWithDOM(float value)
        {
            throw new NotImplementedException("Invalid context");
        }

        public override void AddRule(FuzzyRule rule)
        {
            throw new NotImplementedException("Invalid context");
        }

        #endregion
    }
}