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
        private bool _isRunning;

        #region Constructors

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

        #endregion

        #region Properties of Goal

        public Agent Agent { get; private set; }

        #endregion

        #region Implementation of IScripted

        public ScriptBox Script { get; private set; }

        #endregion

        #region Methods of Goal

        /// <summary>
        /// Raises the <see cref="E:Terminated" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnTerminated(EventArgs e)
        {
            // If the goal was running (not paused) exit the goal.
            if (_isRunning && _onExit != null)
                Agent.TryExecute(_onExit);

            // Terminate all sub goals.
            while (Count > 0)
                Peek().Terminate();

            // Raise the Terminated event.
            if (Terminated != null)
                Terminated(this, e);
        }

        private void GoalTerminated(object sender, EventArgs e)
        {
            // Sanity check.
            if (Count == 0) return;

            // Assuming the terminated goal is on top of the stack; Pop the goal of the stack.
            Pop();
        }

        #endregion

        #region API

        [ScriptingFunction]
        public void AddSubgoal(AMXArgumentList arguments)
        {
            // Check the arguments.
            if (arguments.Length < 1) return;

            var scriptname = arguments[0].AsString();

            // Create the goal with te specified scriptname.
            var goal = new Goal(Agent, scriptname);

            // Add eventual variables to the goal.
            if (arguments.Length > 2)
                DefaultFunctions.SetVariables(goal, arguments, 1);

            AddSubgoal(goal);
        }

        [ScriptingFunction]
        public bool GetSubgoalCount()
        {
            return Count > 0;
        }

        [ScriptingFunction]
        public void TerminateSubgoals()
        {
            while (Count > 0)
                Peek().Terminate();
        }
        #endregion

        #region Implementation of IGoal

        public void Pause()
        {
            if (!_isRunning) return;

            _isRunning = false;

            if (_onExit != null)
                Agent.TryExecute(_onExit);

            if (Count > 0)
                Peek().Pause();
        }

        public void Process(GameTime gameTime)
        {
            // If the goal wasn't running before now, call the OnEnter function.
            if (!_isRunning)
            {
                _isRunning = true;

                if (_onEnter != null)
                    Agent.TryExecute(_onEnter);
            }

            // Call the Update function.
            if (_onUpdate != null)
                Agent.TryExecute(_onUpdate);

            // Update the top goal on the stack.
            if(Count > 0)
                Peek().Process(gameTime);
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
            // If there is a goal on the stack, pause it.
            if (Count > 0)
                Peek().Pause();

            // Activate and store the goal.
            goal.Terminated += GoalTerminated;
            Push(goal);
            goal.Activate();
        }

        [ScriptingFunction]
        public string Name
        {
            get { return _scriptName; }
        }

        public event EventHandler Terminated;

        #endregion

        #region Implementation of IMessageHandler

        public void HandleMessage(int message, int contents)
        {
            if (_onIncomingMessage != null)
            {
                // Call the goal's.
                Script.Push(contents);
                Script.Push(message);
                Agent.TryExecute(_onIncomingMessage);
            }

            // If this goal has subgoals, handle message in the next.
            if (Count > 0)
                Peek().HandleMessage(message, contents);
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