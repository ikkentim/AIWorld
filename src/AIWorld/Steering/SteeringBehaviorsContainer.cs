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
using System.Diagnostics;
using System.Linq;
using AIWorld.Entities;
using AIWorld.Scripting;
using Microsoft.Xna.Framework;

namespace AIWorld.Steering
{
    public class SteeringBehaviorsContainer
    {
        private static readonly Type[] SteeringBehaviors;

        static SteeringBehaviorsContainer()
        {
            SteeringBehaviors = typeof (ISteeringBehavior).Assembly.GetTypes()
                .Where(t => typeof (ISteeringBehavior).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .Where(t => !t.IsInterface)
                .Where(t => t.GetConstructor(new[] {typeof (Agent)}) != null)
                .ToArray();
        }

        public static void Register(Agent agent, ScriptBox script)
        {
            foreach (var behavior in SteeringBehaviors)
            {
                var functionName = string.Format("Add{0}",
                    behavior.Name.EndsWith("SteeringBehavior")
                        ? behavior.Name.Substring(0, behavior.Name.Length - "SteeringBehavior".Length)
                        : behavior.Name);

                var localBehavior = behavior;

                script.Register(functionName, (amx, args) =>
                {
                    var properties = localBehavior.GetProperties()
                        .Where(
                            p =>
                                p.GetCustomAttributes(typeof (SteeringBehaviorArgumentAttribute), true)
                                    .Any())
                        .OrderBy(
                            p =>
                                ((SteeringBehaviorArgumentAttribute)
                                    p.GetCustomAttributes(typeof (SteeringBehaviorArgumentAttribute), true)
                                        .First())
                                    .Index).ToArray();

                    if (args.Length != 1 + properties.Sum(p => p.PropertyType == typeof(Vector3) ? 2 : 1))
                        return -1;

                    var weight = args[0].AsFloat();
                    var idx = 1;

                    var instance = Activator.CreateInstance(localBehavior, agent) as ISteeringBehavior;
                    foreach (var property in properties)
                    {
                        if (property.PropertyType == typeof (int))
                            property.SetValue(instance, args[idx++].AsInt32(), null);
                        else if (property.PropertyType == typeof (bool))
                            property.SetValue(instance, args[idx++].AsInt32() != 0, null);
                        else if (property.PropertyType == typeof (float))
                            property.SetValue(instance, args[idx++].AsFloat(), null);
                        else if (property.PropertyType == typeof (string))
                            property.SetValue(instance, args[idx++].AsString(), null);
                        else if (property.PropertyType == typeof (Vector3))
                            property.SetValue(instance,
                                new Vector3(args[idx++].AsFloat(), 0, args[idx++].AsFloat()),
                                null);
                        else
                            throw new Exception("Invalid type");
                    }

                    return agent.AddSteeringBehavior(weight, instance);
                });
            }
        }
    }
}