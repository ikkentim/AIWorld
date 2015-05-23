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
using AIWorld.Core;
using AIWorld.Helpers;
using AIWorld.Scripting;
using AIWorld.Services;
using AIWorld.Steering;
using AMXWrapper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AIWorld.Entities
{
    /// <summary>
    ///     Represents a scriptable in-game agent.
    /// </summary>
    public class Agent : Entity, IMovingEntity, IScripted, IMessageHandler
    {
        private readonly BasicEffect _basicEffect;
        private readonly ICameraService _cameraService;
        private readonly IConsoleService _consoleService;
        private readonly IGameWorldService _gameWorldService;
        private readonly Stack<IGoal> _goals = new Stack<IGoal>();
        private readonly AMXPublic _onClicked;
        private readonly AMXPublic _onIncomingMessage;
        private readonly AMXPublic _onKeyStateChanged;
        private readonly AMXPublic _onMouseClick;
        private readonly AMXPublic _onUpdate;
        private readonly Stack<Node> _path = new Stack<Node>();

        private readonly Dictionary<string, WeightedSteeringBehavior> _steeringBehaviors =
            new Dictionary<string, WeightedSteeringBehavior>();

        private AudioEmitter _audioEmitter;
        private Model _model;
        private SoundEffect _soundEffect;
        private SoundEffectInstance _soundEffectInstance;
        private float _targetRange;
        private float _targetRangeSquared;
        private Matrix[] _transforms;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Agent" /> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="scriptName">Name of the script.</param>
        /// <param name="position">The position.</param>
        /// <exception cref="ArgumentNullException">scriptName</exception>
        public Agent(Simulation simulation, string scriptName, Vector3 position)
            : base(simulation)
        {
            if (scriptName == null) throw new ArgumentNullException("scriptName");

            // For drawing paths
            _basicEffect = new BasicEffect(GraphicsDevice);

            // Listen to user input
            simulation.KeyStateChanged += game_KeyStateChanged;
            simulation.MouseClick += game_MouseClick;

            // Setup initial values
            ScriptName = scriptName;
            Position = position;
            Heading = Vector3.Right;
            Side = Heading.RotateAboutOriginY(Vector3.Zero, MathHelper.ToRadians(90));

            // Fetch service instances
            var drawingService = simulation.Services.GetService<IDrawingService>();
            _cameraService = simulation.Services.GetService<ICameraService>();
            _gameWorldService = simulation.Services.GetService<IGameWorldService>();
            _consoleService = simulation.Services.GetService<IConsoleService>();

            // Load script
            Script = new ScriptBox("agent", scriptName);
            Script.Register(this, _gameWorldService, _consoleService, drawingService);

            // Load scripting callbacks
            _onUpdate = Script.FindPublic("OnUpdate");
            _onClicked = Script.FindPublic("OnClicked");
            _onMouseClick = Script.FindPublic("OnMouseClick");
            _onKeyStateChanged = Script.FindPublic("OnKeyStateChanged");
            _onIncomingMessage = Script.FindPublic("OnIncomingMessage");
        }

        #region Implementation of IMessageHandler

        /// <summary>
        ///     Is called when a message has been sent to this instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="contents">The contents.</param>
        public void HandleMessage(int message, int contents)
        {
            // Drop the message down the script chain
            if (_onIncomingMessage != null)
            {
                Script.Push(contents);
                Script.Push(message);
                TryExecute(_onIncomingMessage);
            }

            if (_goals.Count > 0)
                _goals.Peek().HandleMessage(message, contents);
        }

        #endregion

        /// <summary>
        ///     Is called when the user has clicked on this instance.
        /// </summary>
        /// <param name="e">The <see cref="MouseClickEventArgs" /> instance containing the event data.</param>
        /// <returns>True if this instance has handled the input; False otherwise.</returns>
        public override bool OnClicked(MouseClickEventArgs e)
        {
            if (_onClicked == null) return false;

            // Push the arguments and call the callback.
            Script.Push(e.Position.Z);
            Script.Push(e.Position.X);
            Script.Push(e.Button);
            return _onClicked.Execute() == 1;
        }

        #region Overrides of GameComponent

        /// <summary>
        ///     Shuts down the component.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            // Stop sounds
            if (_soundEffectInstance != null)
            {
                _soundEffectInstance.Stop(true);
                _soundEffectInstance.Dispose();
            }

            // Dispose of the scriptbox
            if (Script != null)
                Script.Dispose();

            Script = null;

            // Dispose all goals
            foreach (var g in _goals.OfType<IDisposable>())
                g.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        ///     Starts the logic of this instance.
        /// </summary>
        public void Start()
        {
            TryExecuteMain(Script);
        }

        /// <summary>
        ///     Attempts to execute the specified <paramref name="amxPublic" />. If it fails, a message will be printed to the
        ///     in-game
        ///     console.
        /// </summary>
        /// <param name="amxPublic">The AMX public.</param>
        public void TryExecute(AMXPublic amxPublic)
        {
            try
            {
                amxPublic.Execute();
            }
            catch (Exception e)
            {
                _consoleService.WriteLine(Color.Red, e);
            }
        }

        /// <summary>
        ///     Attempts to execute the main function of the specified <paramref name="scriptBox" />. If it fails, a message will
        ///     be
        ///     printed to the in-game console.
        /// </summary>
        /// <param name="scriptBox">The script box.</param>
        public void TryExecuteMain(ScriptBox scriptBox)
        {
            try
            {
                scriptBox.ExecuteMain();
            }
            catch (Exception e)
            {
                _consoleService.WriteLine(Color.Red, e);
            }
        }

        private void game_MouseClick(object sender, MouseClickEventArgs e)
        {
            if (_onMouseClick == null || Script == null) return;

            // Push the arguments and call the callback.
            Script.Push(e.Position.Z);
            Script.Push(e.Position.X);
            Script.Push(e.Button);

            _onMouseClick.Execute();
        }

        private void game_KeyStateChanged(object sender, KeyStateEventArgs e)
        {
            if (_onKeyStateChanged == null || Script == null) return;

            // Allocate an array for new keys and old keys in the abstract machine.
            var newKeys = Script.Allot(e.NewKeys.Length + 1);
            var oldKeys = Script.Allot(e.OldKeys.Length + 1);

            // Copy the keys to the machine.
            for (var i = 0; i < e.NewKeys.Length; i++)
                (newKeys + i).Set((int) e.NewKeys[i]);

            for (var i = 0; i < e.OldKeys.Length; i++)
                (oldKeys + i).Set((int) e.OldKeys[i]);

            (newKeys + e.NewKeys.Length).Set((int) Keys.None);
            (oldKeys + e.OldKeys.Length).Set((int) Keys.None);

            // Push the arguments and call the callback.
            Script.Push(oldKeys);
            Script.Push(newKeys);

            TryExecute(_onKeyStateChanged);

            // Release the arrays.
            Script.Release(newKeys);
            Script.Release(oldKeys);
        }

        private void goal_Terminated(object sender, EventArgs e)
        {
            if (_goals.Count != 0)
                _goals.Pop();
        }

        /// <summary>
        ///     Calculates the steering force based on installed steering behaviors.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        /// <returns>The summed steering force.</returns>
        private Vector3 CalculateSteeringForce(GameTime gameTime)
        {
            var forces = _steeringBehaviors.Values.Aggregate(Vector3.Zero,
                (current, behavior) => current + behavior.Calculate(gameTime));
            return forces.Truncate(MaxForce);
        }

        /// <summary>
        ///     Updates the position.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        private void UpdatePosition(GameTime gameTime)
        {
            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            var steeringForce = CalculateSteeringForce(gameTime);

            var acceleration = steeringForce/Mass;
            Velocity += acceleration*deltaTime;

            Velocity = Velocity.Truncate(MaxSpeed);

            Position += Velocity*deltaTime;

            if (Velocity.LengthSquared() > 0.00001)
            {
                Heading = Vector3.Normalize(Velocity);
                Side = Heading.RotateAboutOriginY(Vector3.Zero, MathHelper.ToRadians(90));
            }
        }

        /// <summary>
        ///     Draws a line between the specified points in the specified color.
        /// </summary>
        /// <param name="point1">The point1.</param>
        /// <param name="point2">The point2.</param>
        /// <param name="color1">The color1.</param>
        /// <param name="color2">The color2.</param>
        private void DrawLine(Vector3 point1, Vector3 point2, Color color1, Color color2)
        {
            var vertices = new[] {new VertexPositionColor(point1, color1), new VertexPositionColor(point2, color2)};
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        #region Overrides of GameComponent

        /// <summary>
        ///     Updates this instance.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            if (_onUpdate != null)
            {
                Script.Push((float) gameTime.ElapsedGameTime.TotalSeconds);
                TryExecute(_onUpdate);
            }

            if (_goals.Count > 0)
                _goals.Peek().Process(gameTime);

            UpdatePosition(gameTime);

            if (_audioEmitter != null && _soundEffectInstance != null)
            {
                _audioEmitter.Position = Position;
                _audioEmitter.Forward = Heading;
                _audioEmitter.Velocity = Velocity;

                _soundEffectInstance.Apply3D(_cameraService.AudioListener, _audioEmitter);
            }

            base.Update(gameTime);
        }

        #endregion

        #region Overrides of DrawableGameComponent

        /// <summary>
        ///     Draws this instance.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Draw(GameTime gameTime)
        {
            if (_model != null)
            {
                foreach (var mesh in _model.Meshes)
                {
                    foreach (var effect in mesh.Effects.Cast<BasicEffect>())
                    {
                        effect.World = _transforms[mesh.ParentBone.Index]*Matrix.CreateRotationY(Heading.GetYAngle())*
                                       Matrix.CreateTranslation(Position);
                        effect.View = _cameraService.View;
                        effect.Projection = _cameraService.Projection;
                        effect.EnableDefaultLighting();
                    }
                    mesh.Draw();
                }
            }

            if (DrawPath)
            {
                _basicEffect.VertexColorEnabled = true;
                _basicEffect.World = Matrix.Identity;
                _basicEffect.View = _cameraService.View;
                _basicEffect.Projection = _cameraService.Projection;

                _basicEffect.CurrentTechnique.Passes[0].Apply();

                var height = new Vector3(0, 0.1f, 0);
                foreach (var node in _path)
                {
                    var localNode = node;
                    if (node.Previous == null) continue;

                    foreach (var edge in node.Where(e => e.Target.Previous == localNode))
                        DrawLine(edge.Target.Position + height, node.Position + height, Color.Red, Color.Red);

                    DrawLine(node.Previous.Position + height, node.Position + height, Color.Blue, Color.Blue);
                }
            }
            base.Draw(gameTime);
        }

        #endregion

        #region Agent API

        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        [ScriptingFunction]
        public override int Id { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to draw the path on the path stack.
        /// </summary>
        [ScriptingFunction]
        public bool DrawPath { get; set; }

        /// <summary>
        ///     Gets or sets the target range used by <see cref="IsInTargetRangeOfPoint(Vector3)" />.
        /// </summary>
        [ScriptingFunction]
        public float TargetRange
        {
            get { return _targetRange; }
            set
            {
                _targetRange = value;
                _targetRangeSquared = value*value;
            }
        }

        /// <summary>
        ///     Gets the script.
        /// </summary>
        public ScriptBox Script { get; private set; }

        /// <summary>
        ///     Clears the path stack.
        /// </summary>
        /// <returns>True on success; False otherwise.</returns>
        [ScriptingFunction]
        public bool ClearPathStack()
        {
            if (_path.Count == 0) return false;

            _path.Clear();
            return true;
        }

        /// <summary>
        ///     Pops a node off the path stack.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>True on success; False otherwise.</returns>
        [ScriptingFunction]
        public bool PopPathNode(out float x, out float y)
        {
            if (_path.Count == 0)
            {
                x = 0;
                y = 0;
                return false;
            }

            var node = _path.Pop();

            x = node.Position.X;
            y = node.Position.Z;
            return true;
        }

        /// <summary>
        ///     Pushes a node to the path stack.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        [ScriptingFunction]
        public void PushPathNode(float x, float y)
        {
            _path.Push(new Node(new Vector3(x, 0, y)));
        }

        /// <summary>
        ///     Peeks at the top node on the path stack.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>True on success; False otherwise.</returns>
        [ScriptingFunction]
        public bool PeekPathNode(out float x, out float y)
        {
            if (_path.Count == 0)
            {
                x = 0;
                y = 0;
                return false;
            }

            var node = _path.Peek();

            x = node.Position.X;
            y = node.Position.Z;
            return true;
        }

        /// <summary>
        ///     Gets the size of the path stack.
        /// </summary>
        /// <returns>The size of the path stack</returns>
        [ScriptingFunction]
        public int GetPathStackSize()
        {
            return _path.Count;
        }

        /// <summary>
        ///     Calculates and pushes a path to the path stack. The path is calculated from the graph with the specified
        ///     <paramref name="key" />.
        /// </summary>
        /// <param name="key">The key of the graph to calculate the path in.</param>
        /// <param name="startx">The start x-coordinate.</param>
        /// <param name="starty">The start y-coordinate.</param>
        /// <param name="endx">The end x-coordinate.</param>
        /// <param name="endy">The end y-coordinate.</param>
        /// <returns>True on success; False otherwise.</returns>
        [ScriptingFunction]
        public bool PushPath(string key, float startx, float starty, float endx, float endy)
        {
            var a = new Vector3(startx, 0, starty);
            var b = new Vector3(endx, 0, endy);

            var l = _path.Count;

            var graph = _gameWorldService[key];
            if (graph == null) return false;

            foreach (var n in _gameWorldService[key].ShortestPath(a, b))
                _path.Push(n);

            return _path.Count > l;
        }

        //TODO: Add path smoothening function

        /// <summary>
        ///     Sets the model.
        /// </summary>
        /// <param name="modelname">The modelname.</param>
        [ScriptingFunction]
        public void SetModel(string modelname)
        {
            _model = Game.Content.Load<Model>(modelname);

            _transforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(_transforms);
        }

        /// <summary>
        ///     Adds a steering behavior.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="type">The type.</param>
        /// <param name="weight">The weight.</param>
        /// <param name="a">Parameter a.</param>
        /// <param name="b">Parameter b.</param>
        /// <param name="c">Parameter c.</param>
        /// <exception cref="ArgumentNullException">key</exception>
        /// <exception cref="Exception">Invalid steering behavior</exception>
        [ScriptingFunction]
        public void AddSteeringBehavior(string key, SteeringBehaviorType type, float weight, float a, float b, float c)
        {
            if (key == null) throw new ArgumentNullException("key");

            switch (type)
            {
                case SteeringBehaviorType.Arrive:
                    _steeringBehaviors[key] =
                        new WeightedSteeringBehavior(new ArriveSteeringBehavior(this, new Vector3(a, 0, b)), weight);
                    break;
                case SteeringBehaviorType.Seek:
                    _steeringBehaviors[key] =
                        new WeightedSteeringBehavior(new SeekSteeringBehavior(this, new Vector3(a, 0, b)), weight);
                    break;
                case SteeringBehaviorType.ObstacleAvoidance:
                    _steeringBehaviors[key] = new WeightedSteeringBehavior(new AvoidObstaclesBehavior(this), weight);
                    break;
                case SteeringBehaviorType.Explore:
                    _steeringBehaviors[key] =
                        new WeightedSteeringBehavior(new ExploreSteeringBehavior(this, new Vector3(a, 0, b)), weight);
                    break;
                case SteeringBehaviorType.Wander:
                    _steeringBehaviors[key] = new WeightedSteeringBehavior(new WanderSteeringBehavior(this, a, b, c),
                        weight);
                    break;
                default:
                    throw new Exception("Invalid steering behavior");
            }
        }

        /// <summary>
        ///     Sets the sound effect.
        /// </summary>
        /// <param name="sound">The sound.</param>
        /// <param name="isLooped">if set to <c>true</c> the sound is looped.</param>
        /// <param name="volume">The volume.</param>
        /// <returns>True on success; False otherwise.</returns>
        [ScriptingFunction]
        public bool SetSoundEffect(string sound, bool isLooped, float volume)
        {
            try
            {
                _audioEmitter = new AudioEmitter
                {
                    Position = Position,
                    Forward = Heading,
                    Up = Vector3.Up,
                    Velocity = Velocity
                };

                _soundEffect = Game.Content.Load<SoundEffect>(sound);
                _soundEffectInstance = _soundEffect.CreateInstance();
                _soundEffectInstance.IsLooped = isLooped;
                _soundEffectInstance.Volume = volume;
                _soundEffectInstance.Apply3D(_cameraService.AudioListener, _audioEmitter);
                _soundEffectInstance.Play();
                return true;
            }
            catch (Exception e)
            {
                _consoleService.WriteLine(Color.Red, e);
                return false;
            }
        }

        /// <summary>
        ///     Removes the sound effect.
        /// </summary>
        [ScriptingFunction]
        public void RemoveSoundEffect()
        {
            if (_soundEffectInstance != null)
                _soundEffectInstance.Dispose();
            _soundEffectInstance = null;

            if (_soundEffect != null)
                _soundEffect.Dispose();
            _soundEffect = null;

            _audioEmitter = null;
        }

        /// <summary>
        ///     Removes the specified steering behavior.
        /// </summary>
        /// <param name="key">The key of the steering behavior.</param>
        /// <returns>True on success; False otherwise.</returns>
        [ScriptingFunction]
        public bool RemoveSteeringBehavior(string key)
        {
            return _steeringBehaviors.Remove(key);
        }

        /// <summary>
        ///     Determines whether this instance is in range of the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="range">The range.</param>
        /// <returns>True if within range; False otherwise.</returns>
        public bool IsInRangeOfPoint(Vector3 point, float range)
        {
            return Vector3.Distance(point, Position) < range;
        }

        /// <summary>
        ///     Determines whether this instance is in target range of the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>True if within range; False otherwise.</returns>
        public bool IsInTargetRangeOfPoint(Vector3 point)
        {
            return Vector3.DistanceSquared(point, Position) < _targetRangeSquared;
        }

        /// <summary>
        ///     Determines whether this instance is in range of the specified point.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <param name="range">The range.</param>
        /// <returns>True if within range; False otherwise.</returns>
        [ScriptingFunction]
        public bool IsInRangeOfPoint(float x, float y, float range)
        {
            return IsInRangeOfPoint(new Vector3(x, 0, y), range);
        }

        /// <summary>
        ///     Determines whether this instance is in target range of the specified point.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <returns>True if within range; False otherwise.</returns>
        [ScriptingFunction]
        public bool IsInTargetRangeOfPoint(float x, float y)
        {
            return IsInTargetRangeOfPoint(new Vector3(x, 0, y));
        }

        /// <summary>
        ///     Gets the distance to the specified point.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <returns>The distance to the specified point.</returns>
        [ScriptingFunction]
        public float GetDistanceToPoint(float x, float y)
        {
            return Vector3.Distance(Position, new Vector3(x, 0, y));
        }

        /// <summary>
        ///     Adds the specified goal to the goal stack.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        [ScriptingFunction]
        public void AddGoal(AMXArgumentList arguments)
        {
            if (arguments.Length < 1) return;

            var scriptname = arguments[0].AsString();

            if (_goals.Count > 0)
            {
                _goals.Peek().Pause();
            }

            var goal = new Goal(this, scriptname);
            _goals.Push(goal);
            goal.Terminated += goal_Terminated;

            if (arguments.Length > 2)
                DefaultFunctions.SetVariables(goal, arguments, 1);

            goal.Activate();
        }

        /// <summary>
        ///     Gets the goal count of the specified <paramref name="goal" />.
        /// </summary>
        /// <param name="goal">The goal.</param>
        /// <param name="includingSubgoals">if set to <c>true</c> the count includes all subgoals.</param>
        /// <returns>The goal count.</returns>
        private int GetGoalCount(IGoal goal, bool includingSubgoals)
        {
            return includingSubgoals ? goal.Sum(g => GetGoalCount(g, includingSubgoals)) : 1;
        }

        /// <summary>
        ///     Gets the goal count.
        /// </summary>
        /// <param name="includingSubgoals">if set to <c>true</c> the count includes all subgoals.</param>
        /// <returns></returns>
        [ScriptingFunction]
        public int GetGoalCount(bool includingSubgoals)
        {
            return _goals.Sum(g => GetGoalCount(g, includingSubgoals));
        }

        #endregion

        #region Implementation of IMovingEntity

        /// <summary>
        ///     Gets the velocity.
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        ///     Gets the mass.
        /// </summary>
        [ScriptingFunction]
        public float Mass { get; set; }

        /// <summary>
        ///     Gets the heading.
        /// </summary>
        [ScriptingFunction]
        public Vector3 Heading { get; private set; }

        /// <summary>
        ///     Gets the side.
        /// </summary>
        public Vector3 Side { get; private set; }

        /// <summary>
        ///     Gets the maximum speed.
        /// </summary>
        [ScriptingFunction]
        public float MaxSpeed { get; set; }

        /// <summary>
        ///     Gets the maximum force.
        /// </summary>
        [ScriptingFunction]
        public float MaxForce { get; set; }

        #endregion

        #region Overrides of Entity

        /// <summary>
        ///     Gets the name of the script.
        /// </summary>
        public string ScriptName { get; private set; }

        /// <summary>
        ///     Gets the position.
        /// </summary>
        [ScriptingFunction]
        public override Vector3 Position { get; set; }

        /// <summary>
        ///     Gets or sets the size.
        /// </summary>
        [ScriptingFunction]
        public override float Size { get; set; }

        #endregion
    }
}