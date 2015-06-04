AIWorld
=======

*An Artificial Intelligence simulation engine*

<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>
<br/>

Tim Potze  
s1058826  
June 4, 2015

*The source code of this project can also be found on
https://github.com/ikkentim/AIWorld*
<br/>
<br/>
<br/>
<br/>
<br/>


Contents
========

- Introduction
- The Simulation Engine
  - Controls
  - Namespacing
  - Entities
  - Scripting
  - Services
  - Steering
  - Path Planning
  - Fuzzy Logic
- Temple of Kotmogu
  - Behavior
  - Controls
  - Fuzzy Logic
- Conclusion
  - Room for Improvements

Introduction
============

For the assignment of *Artificial Intelligence for Games* I've decided to build
a full simulation engine in which simulations can be programmed using a
scripting language. The scripting language which can be used is the
[Pawn Language] which has been developed by Thiadmer Riemersma and is written
in C. The Pawn language has been using by various gaming related projects, such
as the SourceMod and San Andreas Multiplayer game modifications. Since my
project is written in C# and the Pawn abstract machine is written in C, I've
written a wrapper for it.

The solution found within the source(src) directiory contains three projects:

- AIWorld: The simulation engine described in this document.
- amx32: The pawn abstract machine executor.
- AMXWrapper: A wrapper I've written for the abstract machine executor.

This document consists of two parts. The first part explains the how the
simulation engine works. The second part explains how the Temple of Kotmogu
simulation works.

The Simulation Engine
=====================

Controls
--------

By default, a couple of keys have a predefined behavior within the simulation.
By pressing the F5 key the simulation is reloaded. Using the arrow keys the
camera can be moved. By dragging the mouse while pressing the middle or right
mouse button the camera can be rotated. Using the scroll wheel the camera can
zoom in and out. By pressing the Tab key, the console can be shown or hidden.
By pressing the page up/page down keys or by scrolling while the console is
open you can go up and down in the console messages history.

Namespacing
-----------

The AIWorld project contains a number of namespaces:

- AIWorld: Contains the entry point of the application.
- AIWorld.Core: Contains a number of datastructures used troughout the project.
- AIWorld.Drawable: Contains various 2D and 3D drawable compoments.
- AIWorld.Entities: Contains the entity interfaces, base types, and a number of
  entity implementations.
- AIWorld.Events: Contains event arguments objects.
- AIWorld.Fuzzy.x: Contains all classes required for computing fuzzy logic.
- AIWorld.Goals: Contains the classes used for the implementation of goal driven
  behavior.
- AIWorld.Helpers: Contains a number of static helper classes.
- AIWorld.Planes: Contains classes of drawable planes.
- AIWorld.Scripting: Contains classes and interfaces related to scripting.
- AIWorld.Services: Contains every service available in the application.
- AIWorld.Steering: Contains every steering behavior available in the
  simulation.

Entities
--------
There are three classes which implement the `IEntity` interface. The `Agent`,
`Projectile` and `WorldObject` class.

### Agent

Agents are moving entities with a behavior specified by a script. The script can
define the behavior of the agent by manipulating a couple of data containers
within the agent, including a list of active steering behaviors, a stack of path
nodes (See Path Planning chapter), a stack of goals and a dictionary of model
mesh data.

### Projectile

Agents can fire a projectile from a specified position with a specified
velocity. When the projectile hits an entity which implements the `IHitable`
interface it will call the `IHitable.Hit` method on the hittee. If this method
returns true the projectile will remove itself from the world. When spawning a
projectile there is also an option to specify a number of seconds after which
the projectile is automatically deleted to prevent the world from filling up
with thousands of projectiles.

### WorldObject

World objects can be placed by the main script as obstacles for agents and
projectiles. When creating a world object there is an option to specify which
meshes of the model should be visible and to supply a rotation, scale and
translation matrix.

![](https://raw.githubusercontent.com/ikkentim/AIWorld/master/readme_data/Entities.png)

Scripting
---------

AIWorld requires scripts to be written in the Pawn Language. A language guide
can be found in `docs/Pawn_Language_Guide.pdf`.

Every instance of the pawn abstract machine is created by the `ScriptBox` class.
By default only a small number of core native functions (functions defined
outside of the abstract machine) are available to the script. The class contains
three methods to make more native functions available to the script. Using
`ScriptBox.Register(string, AMXNativeFunction)` you can register a function to
the script with the specified function name.
`ScriptBox.Register(AMXNativeFunction)` has the same behavior, but uses the
function name of the specified function itself.
`ScriptBox.Register(object[])` registers every method tagged with the
`ScriptingNativeAttribute` in the specified object instances.

In the engine, there are three types of scripts. Each of these scripts have
access to some specially crafted native functions to allow the script to fulfil
its purpose. Some services (see the *Services* chapter) also provide
functionality to specific scripts.

### Main Script
The main script creates the terrain, objects, agents, graphs and global
variables. It can also handle user input, such as mouse clicks and key presses.
The main script instance is created by the main `Simulation` class. To this
script only a limited number of native functions are available. These functions
can be found in the `bin/include/a_main.inc` file. Functions provided by the
`GameWorldService`, `ConsoleService`, `DrawingService` and `SoundService` are
also available in this script.

### Agent Script
The agent script defines the behavior of an agent. The agent script instance is
created by an instance of the `Agent` class. To this script a large number of
native functions are available. These functions can be used to set properties,
manage the active steering behaviors, manage the goals, change the position and
add path nodes to the path stack (see the *Path Planning* chapter) of the agent.
There are also a number of native functions available for calculating the
distance to a point and transforming vectors between local and world spaces.
These functions can be found in the `bin/include/a_agent.inc` file. Functions
provided by the `ConsoleService`, `DrawingService`, `GameWorldService`,
`SoundService` and `FuzzyModule` (see Fuzzy Logic chapter) are also available in
this script.

### Goal Script
The goal script defines the behavior of an agent while the goal is active. The
goal script instance is created by an instance of the `Goal` class. To this
script a couple of unique native functions are available. These functions can be
used to manage the subgoals of the goal. These functions can be found in the
`bin/include/a_goal.inc` file. All agent script native functions are also
available to a goal script. Functions provided by the `ConsoleService`,
`DrawingService`, `GameWorldService`, `SoundService` and `FuzzyModule` (see
Fuzzy Logic chapter) are also available in this script.

Services
--------

### CameraService

The camera service contains various methods to allow users to smoothly operate
the camera. When the camera is manipulated, it automatically updates a view
matrix. The service also watches the graphics device for changes and updates
the projection accordingly.

### ConsoleService

The console services contains methods to append messages to the console or to
the *chat*. Using the *Tab* key the console can be toggled on and off. The chat,
however, is always visible. The chat can only hold a limited number of messages
which disappear after a few seconds. The console service provides a number of
native functions for scripts to append messages to the console or chat.

### DrawingService

The drawing services provides native functions for scripts to draw shapes or
other components in the simulation. Available components include a line, cone,
cylinder, sphere, 2D (overlay) text and 3D (label) text. When the script creates
a component, a handle to the component is returned. This handle can be used to
update any property of the component using the functions provided by the
service. The drawing logic of the components can be found in the
`AIWorld.Drawable` namespace.

### GameWorldService

The game world service provides methods for finding entities in and adding
entities to the game world. It also provides native functions to scripts for
creating graphs and accessing them, manipulating properties and variables of
world objects and agents, manipulating global variables shared across all
running scripts and sending messages or calls to other scripts.

### ParticleService

The particle service has sadly not been implemented yet. It is meant to allow
scripts to spawn particle effects in the game world.

### SoundService

The sound service provides methods and native functions for scripts to play a
sound effect at a position within the game world.

Steering
--------

The `Agent` class contains a list of active weighted steering behaviors. The
agent script, its goals or its goals' subgoals can add or remove these
steering behaviors. Any class within the assembly which implements the
`ISteeringBehavior` interface will be accessible from the scripts. For every of
these classes, a function called *Add[classname]* will be registered to the
script, where *[classname]* is the name of the class of the steering behavior.
If the class name ends with `SteeringBehavior`, it will be omitted from the
function name.

![](https://raw.githubusercontent.com/ikkentim/AIWorld/master/readme_data/Steering.png)

The different steering behaviors are combined by multiplying the calculated
force by the weight, summing these values and then truncating it to the maximum
force for the agent.

### Alignment

The alignment steering behavior attempts to align the heading of the agent into
the same direction as nearby agents. The script can specify which agents to take
into account by specifying a variable name for the agents to have and the value
it should contain.

### Arrive

The arrive steering behavior attempts to steer the agent towards a specified
target and stopping it at the target position.

### AvoidObstacles

The avoid obstacles steering behavior attempts to avoid the agent hitting solid
entities.

### Cohesion

The cohesion steering behavior attempts to let the agent stay close to nearby
agents. The script can specify which agents to take into account by specifying a
variable name for the agents to have and the value it should contain.

### Evade

The evade steering behavior attempts to keep the agent away from agents within a
specified range. The script can specify which agents to take into account by
specifying a variable name for the agents to have and the value it should
contain.

### Explore

The explore steering behavior attempts to zigzag the agent across the specified
area.

### Flee

The flee steering behavior attempts to move the agent away from a specified
point.

### OffsetPursuit

The offset pursuit steering behavior attempts to tail the specified agent at the
specified offset.

### Pursuit

The pursuit steering behavior attempts to tail the specified agent.

### Seek

The seek steering behavior attempts to go straight towards the specified point
at top speed.

### Separation

The separation steering behavior attempts to keep some distance from nearby
agents. The script can specify which agents to take into account by specifying a
variable name for the agents to have and the value it should contain.

### Stop

The stop steering behavior attempts to stop the agent with the specified
breaking weight.

### Wander

The wander steering behavior attempts to let the agent wander in a random,
natural way.

Path Planning
-------------
When an agent generates a path, the resulting path nodes are pushed to the path
stack in reverse. This way, the script can keep popping nodes off and seeking
towards it until the stack is empty.

Graphs are filled using a self invented algorithm. The algorithm is displayed
in the following pseudo code:
```
FillGraph(name, minX, minY, maxX, maxY, offset)
{
    offsets = {
        Vector2(offset, offset),
        Vector2(-offset, -offset),
        Vector2(offset, -offset),
        Vector2(-offset, offset),
        Vector2(-offset, 0),
        Vector2(offset, 0),
        Vector2(0, -offset),
        Vector2(0, offset)
    }

    for every x between minX and maxX in steps of offset
    {
        for every y between minY and maxY in steps of offset
        {
            point = Vector2(x, y)

            entities = solid entities near point

            for p in point + each offsets
            {
                if a ray cast from point to p hits no entry in entities
                {
                    add point to graph
                }
            }
        }
    }
}
```

The A\* algorithm implemented in the engine works according to the following
pseudo code:
```
ShortestPath(start, finish)
{
    nodes = list of Vector2
    result = list of Vector2

    add node from graph to nodes where node position is start

    set every node's distance in graph to infinity

    while nodes is not empty
    {
        current = node from nodes with lowest (node distance +
            (manhattan distance between node and finish))

        remove current from nodes

        if current position is finish
        {
            return the result
        }

        for each edge in current node
        {
            if current distance + edge distance < edge target distance
            {
                edge target distance = current distance + edge distance
                edge target previous node = current

                add edge target to nodes
            }
        }
    }
}
```

This algorithm build the following path with the algorithm:

![](https://raw.githubusercontent.com/ikkentim/AIWorld/master/readme_data/graph_in_action.jpg)

The blue line is the path the algorithm has generated.

*I meant to display all evaluated nodes using red lines, but due to a lack of
time it currently only shows the nodes whose previous node is within the path.*

Fuzzy Logic
-----------
I've implemented the fuzzy logic classes mostly in the way the book *Programming
Game AI by Example* describes it. I have however moved the data container of
fuzzy rules to the consequence variables of the rule. Because I wanted to create
and use the fuzzy logic from the script, I've written a simple fuzzy logic
interpreter which can be found inside the `FuzzyModule` class.

Temple of Kotmogu
=================

Behavior
--------
[Temple of Kotmogu] is a gametype(battleground) in the popular game *World of
Warcraft*. The rules of the gamemode has been recreated within this simulation.
Two teams of tanks battle to collect the yellow orbs. Every few seconds your
team earns a few points per orb in its possession. The closer the carrier of
the orb is to the center of the map(the center is the most *powerful* area), the
more points it receives per every few seconds. Every tank has a limited number
of ammo and health. The tanks will attempt to shoot down the other team. If a
tank dies and it carried an orb, the orb will be returned to its point of
origin. A few seconds after a tank has been shot down, it will reappear at its
spawn point.

Every few seconds a plane arrives with a carepackage which is dropped at a
random position on the map. The carepackage contains a random amount of ammo or
a repair kit for a tank to increase its health.

The tanks have a number of goals:

- attack: search and destroy the nearest enemy.
- combat: fight tanks within firing range.
- defend: defend the nearest ally with an orb.
- get_carepackage: get the nearest carepackage.
- get_orb: get the nearest orb.
- hold: hold the orb and don't leave the power area.
- powerup: find a more powerful area.
- think: think which goal to work on next.

All goal scripts are heavily commented and can be found in
`bin/kotmogu/tank/goal`.

Controls
--------
By clicking on a tank you can selecting it. Selected tanks display their debug
information (including the active path) on the screen.

By right clicking while holding the left shift while you have selected a tank,
the tank will calculate a path to the clicked point and will navigate to there.

By holding the F9 key all graphs will be displayed on the screen.

Fuzzy Logic
-----------

In the simulation, there are currently 14 fuzzy variables:
- ammo: The amount of ammo the tank has.
- area_power: How powerful the area the tank is in is.
- carepackage: The distance to the nearest carepackage.
- enemy_orbs: The number of orbs the enemies have.
- have_orb: Whether the tank has the orb.
- health: The health of the orb.
- orb: The distance to the nearest orb.
- orbs: The number of orbs the allies have.
- attack_desirability: Desirability to attack.
- carepackage_desirability: Desirability of a carepackage.
- defend_desirability: Desirability to defend.
- hold_desirability: Desirability to hold this position.
- orb_desirability: Desirability of an orb.
- powerup_desirability: Desirability to get to a more powerful area.

*Because of the big number of FLV's and the short amount of time I have to
finish this project, I wont add the graphs for every FLV to this document. The
graphs are created and can be found in `bin/kotmogu/tank/common/fuzzy.inc` at
line 28.*

In the think goal of the tanks I use these fuzzy variables to decide which
action to take:

```
public OnUpdate(Float:elapsed)
{
    // ...

    // Fill the fuzzy variables (e.g. health, ammo, etc.)
    SetFuzzyVariables();

    // These functions are defined in bin/kotmogu/tank/common/fuzzy.inc
    new Float:want_orb = GetOrbDesirability();
    new Float:want_powerup = GetPowerupDesirability();
    new Float:want_carepackage = GetCarepackageDesirability();
    new Float:want_defend = GetDefendDesirability();
    new Float:want_attack = GetAttackDesirability();
    new Float:want_hold = GetHoldDesirability();

    // Macro which returns true if the specified desirability is the highest of
    // all desirabilities
    #define BEST_DESIRABILITY(%1) (%1 >= want_powerup && \
        %1 >= want_carepackage && %1 >= want_defend && \
        %1 >= want_attack && %1 >= want_hold)

    if(BEST_DESIRABILITY(want_orb))
        AddSubgoal("kotmogu/tank/goal/get_orb");
    else if(BEST_DESIRABILITY(want_powerup))
        AddSubgoal("kotmogu/tank/goal/powerup");
    else if(BEST_DESIRABILITY(want_carepackage))
        AddSubgoal("kotmogu/tank/goal/get_carepackage");
    else if(BEST_DESIRABILITY(want_hold))
        AddSubgoal("kotmogu/tank/goal/hold");
    else if(BEST_DESIRABILITY(want_defend))
        AddSubgoal("kotmogu/tank/goal/defend");
    else if(BEST_DESIRABILITY(want_attack))
        AddSubgoal("kotmogu/tank/goal/attack");

    // ...
}
```


Conclusion
==========

Due to my idea of building a whole simulation engine instead of just this one
simulation, I've had to spend a lot of time working on this project. Because it
was such a big project and the time was limited there are some things I've not
had time for to implement, improve or fix.

I have, however, had lots of fun working on this project and I have learned
loads of stuff about artificial intelligence in games. I've no encountered any
big problems in this project. The biggest challenge has been to write a wrapper
for the pawn abstract machine which in the end works perfectly and very fast.

Room for Improvements
---------------------

*A number of the following items are also annotated in the source code.*

- After you navigate a tank to a position by shift-right clicking in the
  simulation, the stack of goals in the debug overlay looks weird. I've not had
  time to look into this.
- Tanks shoot at the nearest enemy, even if there is a friendly tank or a tree
  in between them.
- Cohesion, alignment and/or separation can cause tanks to move in odd ways if
  nearby tanks are standing still or if there are no nearby tanks.
- Not all of the official [Temple of Kotmogu] rules have been implemented,
  including "Delivering a killing blow to one of your enemies will net your team
  a significant number of victory points." and "Orb-holders deal and receive
  increased damage, and the healing they receive is reduced. These effects are
  minor at first, but they increase quickly over time while the orb-holder
  lives.".
- I've started of documenting every method and function, but do to time pressure
  not all methods and functions have proper documentation anymore, both in the
  script and the engine.
- The powerup goal tries to reach the other side of the map to power up. This
  normally results in the tank driving into a area with a higher power anyways,
  but occasionally it does not, which can result in the tank being stuck in the
  powerup goal.
- Sometimes tanks can get stuck behind an obstacle for a while.
- Accessing an entity by id in the `GameWorldService` makes the service loop
  trough all the entities until it has found the entity with the specified id.
  This has a speed of O(n) It would be much better if the entities are put in
  an `AIWorld.Core.Pool<T>` as well, which can get the associated object of an
  id in O(1) time.
- Every time a model, sound or texture is loaded, it's loaded from
  `Simulation.Content` again. I expect this class to read the object from the
  file after every call. It would be much better if the engine keeps these
  instances in memory. The same goes for scripts. Scripts are loaded very often
  and are loaded from the disk every time.
- The fuzzy logic used in the simulation could use lots of improvements. I have
  not spend a lot of time on designing the FLVs and the rules.

Sources
=======

The source code and guides of the [Pawn Language] can be found on:  
http://www.compuphase.com/pawn/pawn.htm

The rules of the [Temple of Kotmogu] can be found on:  
http://us.battle.net/wow/en/game/pvp/battlegrounds/temple-of-kotmogu

Various models and sounds were derived from:  
http://opengameart.org/

During research I've used the code of `Programming Game AI by Example` which can be found on:  
https://github.com/wangchen/Programming-Game-AI-by-Example-src


[Pawn Language]: http://www.compuphase.com/pawn/pawn.htm
[Temple of Kotmogu]: http://us.battle.net/wow/en/game/pvp/battlegrounds/temple-of-kotmogu

