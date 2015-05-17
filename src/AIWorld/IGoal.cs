using System;
using System.Collections.Generic;
using AIWorld.Scripting;
using Microsoft.Xna.Framework;

namespace AIWorld
{
    public interface IGoal : IEnumerable<IGoal>, IScriptingNatives, IMessageHandler
    {
        string Name { get; }
        string CurrentName { get; }
        void Activate();
        void Process(GameTime gameTime);
        void Pause();
        void Terminate();
        void AddSubgoal(IGoal goal);
        event EventHandler Terminated;
    }
}