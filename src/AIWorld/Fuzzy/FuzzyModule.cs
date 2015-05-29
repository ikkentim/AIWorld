// AIWorld
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
using System.Linq;
using System.Text.RegularExpressions;
using AIWorld.Fuzzy.Sets;
using AIWorld.Fuzzy.Terms;
using AIWorld.Scripting;
using AIWorld.Services;
using Microsoft.Xna.Framework;

namespace AIWorld.Fuzzy
{
    public class FuzzyModule : IScriptingNatives
    {
        private readonly IConsoleService _consoleService;
        private Tuple<string, FuzzyVariable>[] _variables = new Tuple<string, FuzzyVariable>[1];

        public FuzzyModule(IConsoleService consoleService)
        {
            if (consoleService == null) throw new ArgumentNullException("consoleService");
            _consoleService = consoleService;
        }

        private int GetFreeSlot<T>(ref T[] arr) where T : class
        {
            if (arr == null) throw new ArgumentNullException("arr");
            for (var i = 0; i < arr.Length; i++)
                if (arr[i] == null)
                    return i;

            var len = arr.Length;
            var tmp = new T[len*2];
            Array.Copy(arr, tmp, len);

            arr = tmp;
            return len;
        }

        [ScriptingFunction]
        public int CreateFuzzyVariable(string name)
        {
            var slot = GetFreeSlot(ref _variables);
            _variables[slot] = new Tuple<string, FuzzyVariable>(name, new FuzzyVariable());

            return slot;
        }

        [ScriptingFunction]
        public bool IsValidFuzzyVariable(int slot)
        {
            return slot >= 0 && slot < _variables.Length && _variables[slot] != null;
        }

        [ScriptingFunction]
        public void DeleteFuzzyVariable(int slot)
        {
            if (IsValidFuzzyVariable(slot))
                _variables[slot] = null;
        }

        private FuzzyTerm CreateTerm(string expression)
        {
            if (expression == null) throw new ArgumentNullException("expression");

            return CreateTerm(new Stack<string>(expression.Split(' ').Where(k => !string.IsNullOrEmpty(k))));
        }

        private FuzzyTerm CreateTerm(Stack<string> stack)
        {
            if (stack == null)
                throw new ArgumentNullException("stack");

            var unaryTerms = new Dictionary<string, Func<FuzzyTerm, FuzzyTerm>>();
            var binaryTerms = new Dictionary<string, Func<FuzzyTerm, FuzzyTerm, FuzzyTerm>>();

            unaryTerms["very"] = t => t.Very();
            unaryTerms["fairly"] = t => t.Fairly();
            binaryTerms["&&"] = (l, r) => l && r;
            binaryTerms["and"] = (l, r) => l && r;
            binaryTerms["||"] = (l, r) => l || r;
            binaryTerms["or"] = (l, r) => l || r;

            FuzzyTerm term = null;

            while (stack.Any())
            {
                var keyword = stack.Pop();
                var lowerKeyword = keyword.ToLower();

                // Initial term was set...
                if (term != null)
                {
                    if (unaryTerms.ContainsKey(lowerKeyword))
                    {
                        term = unaryTerms[lowerKeyword](term);
                        continue;
                    }
                    if (binaryTerms.ContainsKey(lowerKeyword))
                    {
                        var otherTerm = CreateTerm(stack);

                        if (otherTerm == null)
                            return null;

                        term = binaryTerms[lowerKeyword](otherTerm, term);
                        continue;
                    }

                    _consoleService.WriteLine(Color.Red, string.Format("ERROR: Undefined symbol {0}.", keyword));
                    return null;
                }

                // Set initial term.
                if (!stack.Any())
                {
                    _consoleService.WriteLine(Color.Red,
                        "ERROR: Unexpected end of expression. Found expected variable.");
                    return null;
                }

                var fuzzyVariable = _variables.Where(v => v!= null).FirstOrDefault(v => v.Item1 == keyword);

                if (fuzzyVariable == null)
                {
                    _consoleService.WriteLine(Color.Red,
                        string.Format("ERROR: Variable {0} does not exist.", keyword));
                    return null;
                }

                var set = stack.Pop();
                term = fuzzyVariable.Item2[set];

                if (term != null) continue;

                _consoleService.WriteLine(Color.Red,
                    string.Format("ERROR: Set {0} in {1} does not exist.", set, keyword));
                return null;
            }


            if (term == null)
            {
                _consoleService.WriteLine(Color.Red, "ERROR: Invalid expression.");
                return null;
            }

            return term;
        }

        [ScriptingFunction]
        public bool AddFuzzyRule(string rule)
        {
            var regex = new Regex(@"^IF (.*) THEN (.*)$");
            var match = regex.Match(rule.Trim());

            if (!match.Success) return false;

            FuzzyTerm antecent = CreateTerm(match.Groups[1].Value);
            if (antecent == null) return false;

            FuzzyTerm consequence = CreateTerm(match.Groups[2].Value);
            if (consequence == null) return false;

            FuzzyRule.Create(antecent, consequence);
            return true;
        }

        [ScriptingFunction]
        public bool Fuzzify(int slot, float value)
        {
            if (!IsValidFuzzyVariable(slot)) return false;

            _variables[slot].Item2.Fuzzify(value);
            return true;
        }

        [ScriptingFunction]
        public float Defuzzify(int slot)
        {
            if (!IsValidFuzzyVariable(slot)) return 0;
            return _variables[slot].Item2.Defuzzify();
        }

        [ScriptingFunction]
        public bool AddFuzzyLeftShoulder(int slot, string name, float left, float middle, float right)
        {
            if (!IsValidFuzzyVariable(slot)) return false;

            _variables[slot].Item2.Add(name, new FuzzyLeftShoulder(left, middle, right));
            return true;
        }

        [ScriptingFunction]
        public bool AddFuzzyRightShoulder(int slot, string name, float left, float middle, float right)
        {
            if (!IsValidFuzzyVariable(slot)) return false;

            _variables[slot].Item2.Add(name, new FuzzyRightShoulder(left, middle, right));
            return true;
        }

        [ScriptingFunction]
        public bool AddFuzzyTrapezium(int slot, string name, float left, float middleLeft, float middleRight,
            float right)
        {
            if (!IsValidFuzzyVariable(slot)) return false;

            _variables[slot].Item2.Add(name, new FuzzyTrapezium(left, middleLeft, middleRight, right));
            return true;
        }

        [ScriptingFunction]
        public bool AddFuzzyTriangle(int slot, string name, float left, float middle, float right)
        {
            if (!IsValidFuzzyVariable(slot)) return false;

            _variables[slot].Item2.Add(name, new FuzzyTriangle(left, middle, right));
            return true;
        }
    }
}