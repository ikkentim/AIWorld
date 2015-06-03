using System;
using System.Collections.Generic;
using AIWorld.Entities;
using AIWorld.Scripting;
using Microsoft.Xna.Framework;

namespace AIWorld.Goals
{
    public interface IGoal : IEnumerable<IGoal>, IMessageHandler
    {
        string Name { get; }
        void Activate();
        void Process(GameTime gameTime);
        void Pause();
        void Terminate();
        void AddSubgoal(IGoal goal);
        event EventHandler Terminated;
    }
}