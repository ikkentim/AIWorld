using System;
using System.Collections.Generic;
using AIWorld.Scripting;

namespace AIWorld
{
    public interface IGoal : IEnumerable<IGoal>, IScriptingNatives, IMessageHandler
    {
        string Name { get; }
        string CurrentName { get; }
        void Activate();
        void Process();
        void Pause();
        void Terminate();
        void AddSubgoal(IGoal goal);
        event EventHandler Terminated;
    }
}