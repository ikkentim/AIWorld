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

namespace AIWorld.Fuzzy.Sets
{
    public abstract class FuzzySet : FuzzyTerm
    {
        private float _dom;

        protected FuzzySet(float representativeValue)
        {
            RepresentativeValue = representativeValue;
        }

        public FuzzyVariable Variable { get; set; }
        public float RepresentativeValue { get; private set; }
        public abstract float Min { get; }
        public abstract float Max { get; }
        public abstract float CalculateDOM(float value);

        #region Overrides of FuzzyTerm

        public override float DOM
        {
            get { return _dom; }
        }

        public void SetDOM(float value)
        {
            if (value < 0 || value > 1)
                throw new ArgumentOutOfRangeException("value", value, "Must be within range 0.0 - 1.0");

            _dom = value;
        }

        public override void ClearDOM()
        {
            SetDOM(0);
        }

        public override void OrWithDOM(float value)
        {
            if (value > _dom) SetDOM(value);
        }


        public override void AddRule(FuzzyRule rule)
        {
            if(Variable == null)
                throw new Exception("Set not bound to variable");

            Variable.Add(rule);
        }
        #endregion

    }
}