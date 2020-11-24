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
using SharpLogger;

namespace SharpGameTest
{
	public class TestState : GameState
	{
		// Here we set Storable to return true to indicate our state can be stored behind another
		// state and will not be destroyed when pushing a new state to the stack.
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

			return Logger.LogReturn( "Test state loaded successfully.", true );
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
				// Here we use the state manager to set a new active state. Since Storable is
				// true, this state will not be destroyed and will be stored behind the new
				// state.
				Manager.Push( new TestState2() );
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
			Logger.Log( "Test state stored." );
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

			Logger.Log( "Test state restored." );
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

			// Here we access the running game through the state manager using GameState.Manager.Game.
			View view = Manager.Game.Window.GetView();
			Vector2f size = view.Size / 2.0f;

			for( uint i = 0; i < 4; i++ )
				m_verts[ i ] = new Vertex( new Vector2f( view.Center.X + ( i < 1 || i > 2 ? -( size.X / 2.0f ) : ( size.X / 2.0f ) ),
														 view.Center.Y + ( i < 2 ? -( size.Y / 2.0f ) : ( size.Y / 2.0f ) ) ),
										   Color.Green );

			m_timer = new Clock();
			return Logger.LogReturn( "Test state 2 loaded successfully.", true );
		}

		public override void Update( float dt )
		{
			if( m_timer.ElapsedTime.AsSeconds() >= 2.0f )
			{
				// Here we pop the state off the top of the state stack and destroy it, returning
				// back to a previously restored state.
				Manager.Pop();
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
