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

namespace AIWorld.Fuzzy.Sets
{
    public class FuzzyRightShoulder : FuzzySet
    {
        private readonly float _left;
        private readonly float _peak;
        private readonly float _right;

        public FuzzyRightShoulder(float left, float middle, float right)
            : base((right - middle)/2 + middle)
        {
            _peak = middle;
            _left = left;
            _right = right;
        }

        #region Overrides of FuzzySet

        public override float Min
        {
            get { return _left; }
        }

        public override float Max
        {
            get { return _right; }
        }

        public override float CalculateDOM(float value)
        {
            if (value < _left || value > _right) return 0;

            if (value < _peak)
                return _peak == _left ? 1 : (value - _left)/(_peak - _left);

            return 1;
        }

        #endregion
    }
}