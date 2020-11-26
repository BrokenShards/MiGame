﻿# SharpGame
A basic game library for use with SFML.Net.

## Dependencies
- SFML.Net `https://github.com/graphnode/SFML.Net.git`
- SharpLogger `https://github.com/BrokenShards/SharpLogger.git`
- SharpSerial `https://github.com/BrokenShards/SharpSerial.git`
- SharpTest `https://github.com/BrokenShards/SharpTest.git`

## Usage
In SharpGame, games are made up of game states that are managed by a state manager, owned by a game
window.
<br>
A game state, as the name implies, represents a state of your game such as a title screen
and contains any needed assets and objects for that state.
<br>
A state manager contains a stack of game states, the state on the top is the currently active 
state. When a new state is pushed to the top, the current state is kept on the stack and stored if
it is `Storable` or removed and disposed if it is not, the new state is then loaded and added. When
a state is removed from the stack, the state underneath is restored and becomes active again.
<br>
A game window contains the render window and state manager used to run a game. Simply create a new
`GameWindow` and call `Run(IGameState)`, passing it the beginning game state of your game, for
example, if the beginning state of your game was a class called `StartState`, then you would run
the game like so:

```
int result = 0;

using( GameWindow game = new GameWindow() )
	result = game.Run( new StartState() );

if( result != 0 )
	Console.WriteLine( "Game ran unsuccessfully." );
```

The `GameWindow` class can also be inherited from to easily integrate other systems and libraries,
check the documentation for more information.
<br>
Please see `Example.cs` in the test project for usage examples.

## Changelog

### Version 0.2.0
- `GameWindow.OnUpdate(float)` has been replaced with `GameWindow.PreUpdate(float)` for updating
  logic before the current game state and `GameWindow.PostUpdate(float)` for updating logic after.
- `GameWindow.OnDraw()` has been replaced with `GameWindow.PreDraw()` for drawing content 
  before/underneath the current game state and `GameWindow.PostUpdate(float)` for drawing content
  after/above.
- When calling `StateManager.Push(IGameState)`, `GameState.OnStore()` is called for the current
  state before `GameState.OnLoad()` is called for the new state, as originally intended.  

### Version 0.1.0
- Initial release.
