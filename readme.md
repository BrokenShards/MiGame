# SharpGame
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
and contains any needed assets and objects.
<br>
A state manager is a stack of game states, the state on the top is the currently active state. When
a state is pushed to the top, the current state is either stored, or popped off the stack and 
disposed of before the new state is loaded and added. When a state is popped off the stack and
disposed of, the state underneath is restored and becomes the active state.
<br>
A game window contains the render window and state manager used to run a game. Simply create a new
`GameWindow` and call `Run(IGameState)`, passing it the game state your game starts from, for
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

### Version 0.1.0
- Initial release.
