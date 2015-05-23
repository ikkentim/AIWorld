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
    public class FuzzyVery : FuzzyTerm
    {
        private readonly FuzzyTerm _set;

        public FuzzyVery(FuzzyTerm set)
        {
            if (set == null) throw new ArgumentNullException("set");
            _set = set;
        }

        #region Implementation of FuzzyTerm

        public override float DOM
        {
            get { return _set*_set; }
        }

        public override void ClearDOM()
        {
            _set.ClearDOM();
        }

        public override void OrWithDOM(float value)
        {
            _set.OrWithDOM(value*value);
        }

        public override void AddRule(FuzzyRule rule)
        {
            _set.AddRule(rule);
        }

        #endregion
    }
}