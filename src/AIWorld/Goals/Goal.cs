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
using System.Linq;
using AIWorld.Entities;
using AIWorld.Fuzzy;
using AIWorld.Scripting;
using AIWorld.Services;
using AIWorld.Steering;
using AMXWrapper;
using Microsoft.Xna.Framework;

namespace AIWorld.Goals
{
    public class Goal : Stack<IGoal>, IGoal, IScripted, IDisposable
    {
        private readonly AMXPublic _onUpdate;
        private readonly AMXPublic _onEnter;
        private readonly AMXPublic _onExit;
        private readonly AMXPublic _onIncomingMessage;
        private readonly string _scriptName;
        private bool _isActive;

        public Goal(Agent agent, string scriptName)
        {
            if (agent == null) throw new ArgumentNullException("agent");
            if (scriptName == null) throw new ArgumentNullException("scriptName");

            Agent = agent;
            _scriptName = scriptName;

            Script = new ScriptBox(scriptName);
            Script.Register(this, agent, agent.Game.Services.GetService<IGameWorldService>(),
                new FuzzyModule(agent.Game.Services.GetService<IConsoleService>()),
                agent.Game.Services.GetService<IConsoleService>(),
                agent.Game.Services.GetService<ISoundService>());

            SteeringBehaviorsContainer.Register(agent, Script);

            _onUpdate = Script.FindPublic("OnUpdate");
            _onEnter = Script.FindPublic("OnEnter");
            _onExit = Script.FindPublic("OnExit");
            _onIncomingMessage = Script.FindPublic("OnIncomingMessage");
        }

        public Agent Agent { get; private set; }

        #region Implementation of IScripted

        public ScriptBox Script { get; private set; }

        #endregion

        protected void OnTerminated(EventArgs e)
        {
            if (_isActive && _onExit != null)
                Agent.TryExecute(_onExit);

            while(Count > 0)
                Peek().Terminate();

            if (Terminated != null)
                Terminated(this, e);
        }

        private void GoalTerminated(object sender, EventArgs e)
        {
            // Sanity check
            if (Count == 0) return;

            // Remove the goal from the stack.
            Pop();

            // If there are any remaining goals, activate the next.
            if (Count != 0)
                Peek().Activate();
        }

        [ScriptingFunction]
        public void AddSubgoal(AMXArgumentList arguments)
        {
            if (arguments.Length < 1) return;

            var scriptname = arguments[0].AsString();

            var goal = new Goal(Agent, scriptname);
            goal.Terminated += GoalTerminated;
            Push(goal);

            if (arguments.Length > 2)
                DefaultFunctions.SetVariables(goal, arguments, 1);

            goal.Activate();
        }

        #region Implementation of IGoal

        public void Pause()
        {
            if (_isActive)
            {
                _isActive = false;

                if (_onExit != null)
                    Agent.TryExecute(_onExit);
            }
            else
            {
                if (Count > 0)
                {
                    Peek().Pause();
                }
            }
        }

        public void Process(GameTime gameTime)
        {
            // If this goal has subgoals, process the next.
            if (Count > 0)
            {
                if (_isActive && _onExit != null)
                {
                    _isActive = false;
                    Agent.TryExecute(_onExit);
                }
                else
                {
                    Peek().Process(gameTime);
                }
            }
            else if (_onUpdate != null)
            {
                if (!_isActive && _onEnter != null)
                {
                    _isActive = true;
                    Agent.TryExecute(_onEnter);
                }
                else
                {
                    Script.Push((float)gameTime.ElapsedGameTime.TotalSeconds);
                    Agent.TryExecute(_onUpdate);
                }
            }
        }

        public void Activate()
        {
            Agent.TryExecuteMain(Script);
        }

        [ScriptingFunction]
        public void Terminate()
        {
            OnTerminated(EventArgs.Empty);
        }

        public void AddSubgoal(IGoal goal)
        {
            // Activate and store the goal.
            goal.Terminated += GoalTerminated;
            Push(goal);
            goal.Activate();
        }

        public IGoal GetActiveGoal()
        {
            return Count > 0 ? Peek().GetActiveGoal() : this;
        }

        [ScriptingFunction]
        public string Name
        {
            get { return _scriptName; }
        }

        public string CurrentName
        {
            get
            {
                return Count > 0 ? Peek().CurrentName : Name;
            }
        }

        public event EventHandler Terminated;

        #endregion

        #region Implementation of IMessageHandler

        public void HandleMessage(int message, int contents)
        {
            // If this goal has subgoals, handle message in the next.
            if (Count > 0)
            {
                Peek().HandleMessage(message, contents);
            }
            else if (_onIncomingMessage != null)
            {
                // Call the goal's.
                Script.Push(contents);
                Script.Push(message);
                Agent.TryExecute(_onIncomingMessage);
            }
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if(Script != null)
                Script.Dispose();
            Script = null;
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return base.ToString() + "{" + Name + "}";
        }

        #endregion
    }
}