VGame
=====

make awesome vectory games in C#, fast


Overview
--------
VGame is a super easy-to-use framework to make cross-platform vector art-based games in C#.

If you're anything like me, you're a game designer/programmer with little-to-no art skills who wants to prototype some
video games, fast. You don't want to mess around with low-level graphics calls and crap. You're sick and tired of making
roguelikes; while you liked fooling around with them because you can't make graphics and you can make code, you're
through with iterating on the same old formulas over and over again.

Maybe you want to make an action game, or maybe you want to just make some cool vectory shit happen on the screen.
Either way, VGame is the framework for you.


Features
--------

- uses SDL and Mono (so it's cross-platform)
- game state system
- menu system
- input system - handles key and mouse button press, down, and release states for you
- Source enginesque command system, including key and mouse button bindings for commands, a command console, and console
  variables
- borderless windowed fullscreen mode support


Notice
------

VGame is super early and *super* undocumented. I'm basically making as I work on my other project,
[Arena](http://github.com/adamrezich/arena), implementing stuff and adding features as I need them.

You really oughtn't use it for your own stuff yet.

Right now, Cairo doesn't have support for OpenGL yet... so you really can't draw to 1920x1080 surfaces without
everything getting hella slow, because your CPU can only do so much and your system RAM is only so fast. Experimentation
with Cairo's in-development OpenGL stuff will begin... eventually.