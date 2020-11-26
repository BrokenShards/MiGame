////////////////////////////////////////////////////////////////////////////////
// Example.cs 
////////////////////////////////////////////////////////////////////////////////
//
// SharpGame - A simple game framework for use with SFML.Net.
// Copyright (C) 2020 Michael Furlong <michaeljfurlong@outlook.com>
//
// This program is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the Free 
// Software Foundation, either version 3 of the License, or (at your option) 
// any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for 
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program. If not, see <https://www.gnu.org/licenses/>.
//
////////////////////////////////////////////////////////////////////////////////

using System;

using SFML.Graphics;
using SFML.System;
using SharpGame;

namespace SharpGameTest
{
	// In this quick example, we implement two different game states, use the state manager to
	// switch between them and the game window to run them.

	public class TestState : GameState
	{
		// Here we set Storable to true to indicate our state can be stored behind another and will
		// not be removed when a new state is added.
		public override bool Storable
		{
			get { return true; }
		}

		// Here we set up our game state and load any needed content.
		public override bool LoadContent()
		{
			m_verts = new VertexArray( PrimitiveType.Quads, 4 );

			// Here we access the running game through the state manager using GameState.Manager.Game.
			View     view = Manager.Game.Window.GetView();
			Vector2f size = view.Size / 2.0f;
			
			for( uint i = 0; i < 4; i++ )
				m_verts[ i ] = new Vertex( new Vector2f( view.Center.X + ( i < 1 || i > 2 ? -( size.X / 2.0f ) : ( size.X / 2.0f ) ),
														 view.Center.Y + ( i < 2 ? -( size.Y / 2.0f ) : ( size.Y / 2.0f ) ) ),
										   new Color( 0, 0, 0, 255 ) );

			Console.WriteLine( "Test state loaded successfully." );
			return true;
		}

		// Update is called every frame and is where your logic should go.
		public override void Update( float dt )
		{
			bool done = false;

			for( uint i = 0; i < 4; i++ )
			{
				Color col = m_verts[ i ].Color;

				col.R += 5;
				col.G += 5;
				col.B += 5;

				done = col.R == 255;

				Vertex vert = m_verts[ i ];
				vert.Color = col;
				m_verts[ i ] = vert;
			}

			if( done )
			{
				// Here we call StateManager.Push(IGameState) to add a new state to top of the
				// stack and load it. Since Storable is true, this state will not be destroyed and
				// will be stored behind the new state.
				if( !Manager.Push( new TestState2() ) )
				{
					// Here adding the new state failed, so TestState2.LoadContent() must have
					// failed.
					throw new Exception( "Failed switching to test state 2." );
				}
			}
		}
		// Draw is called every frame after update and is where your draw calls should go.
		public override void Draw( RenderTarget target, RenderStates states )
		{
			m_verts.Draw( target, states );
		}

		// Dispose is called before the state is destroyed, dispose of any loaded content here. 
		public override void Dispose()
		{
			m_verts.Dispose();
		}

		// OnStore is called when the state is stored behind a new state and is no longer active.
		// This should be used to set the state up for storage.
		public override void OnStore()
		{
			Console.WriteLine( "Test state stored." );
		}
		// OnRestore is called when the state becomes active again. This should be used to
		// reinitialise the state, ready to run again.
		public override void onRestore()
		{
			for( uint i = 0; i < 4; i++ )
			{
				Vertex vert = m_verts[ i ];
				vert.Color = new Color( 0, 0, 0, 255 );
				m_verts[ i ] = vert;
			}

			Console.WriteLine( "Test state restored." );
		}

		private VertexArray m_verts;
	}
	public class TestState2 : GameState
	{
		public override bool Storable
		{
			get { return false; }
		}

		public override bool LoadContent()
		{
			m_verts = new VertexArray( PrimitiveType.Quads, 4 );

			View view = Manager.Game.Window.GetView();
			Vector2f size = view.Size / 2.0f;

			for( uint i = 0; i < 4; i++ )
				m_verts[ i ] = new Vertex( new Vector2f( view.Center.X + ( i < 1 || i > 2 ? -( size.X / 2.0f ) : ( size.X / 2.0f ) ),
														 view.Center.Y + ( i < 2 ? -( size.Y / 2.0f ) : ( size.Y / 2.0f ) ) ),
										   Color.Green );

			m_timer = new Clock();
			Console.WriteLine( "Test state 2 loaded successfully." );
			return true;
		}

		public override void Update( float dt )
		{
			// Here we want to pop back to the previous state after 2 seconds.
			if( m_timer.ElapsedTime.AsSeconds() >= 2.0f )
			{
				// Ensure there is a state to pop back to, if not, we call Reset to remove all 
				// stored states and add a new one.
				if( Manager.Count == 1 )
				{
					if( !Manager.Reset( new TestState() ) )
						throw new Exception( "Failed resetting back to test state." );
				}
				else
				{
					// We call StateManager.Pop() to remove the current game state and restore the
					// previously stored state.
					Manager.Pop();
				}
			}
		}
		// Draw is called every frame after update and is where your draw calls should go.
		public override void Draw( RenderTarget target, RenderStates states )
		{
			m_verts.Draw( target, states );
		}

		// Dispose is called before the state is destroyed, dispose of any loaded content here. 
		public override void Dispose()
		{
			m_verts.Dispose();
		}

		private VertexArray m_verts;
		private Clock       m_timer;
	}

	public static class Example
	{
		public static void RunExample()
		{
			// Here we use the default game window implementation and set it to run, starting with
			// our test state.
			using( GameWindow game = new GameWindow() )
				game.Run( new TestState() );
		}

		public static void Main( string[] args )
		{
			RunExample();
		}
	}
}
