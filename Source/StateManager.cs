////////////////////////////////////////////////////////////////////////////////
// StateManager.cs 
////////////////////////////////////////////////////////////////////////////////
//
// MiGame - A simple game framework for use with SFML.Net.
// Copyright (C) 2021 Michael Furlong <michaeljfurlong@outlook.com>
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
using System.Collections.Generic;

using SFML.Graphics;
using SFML.Window;

using MiCore;

namespace MiGame
{
	/// <summary>
	///   Manages running game states.
	/// </summary>
	public sealed class StateManager : Drawable, IDisposable
	{
		/// <summary>
		///   Constructor.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		///   If <paramref name="game"/> is null.
		/// </exception>
		public StateManager( GameWindow game )
		{
			if( game is null )
				throw new ArgumentNullException( nameof( game ) );

			m_states = new Stack<IGameState>();
			Game     = game;
		}

		/// <summary>
		///   The game window.
		/// </summary>
		public GameWindow Game
		{
			get; init;
		}

		/// <summary>
		///   If the manager does not contain a game state.
		/// </summary>
		public bool Empty
		{
			get { return Count is 0; }
		}
		/// <summary>
		///   The amount of game states the manager currently contains.
		/// </summary>
		public int Count
		{
			get { return m_states.Count; }
		}

		/// <summary>
		///   Pushes a new state to the top of the stack and attempts to load its content.
		/// </summary>
		/// <param name="state">
		///   The game state to switch to.
		/// </param>
		/// <returns>
		///   True if the game state is valid and was loaded successfully.
		/// </returns>
		public bool Push( IGameState state )
		{
			if( state is null )
				return Logger.LogReturn( "Cannot push null game state.", false, LogType.Error );

			if( !Empty )
			{
				m_states.Peek().UnsubscribeEvents();

				if( m_states.Peek().Storable )
					m_states.Peek().OnStore();
				else
				{
					m_states.Peek().Manager = null;
					m_states.Pop().Dispose();
				}
			}

			state.Manager = this;
			state.SubscribeEvents();

			if( !state.LoadContent() )
			{
				state.UnsubscribeEvents();
				state.Manager = null;
				state.Dispose();
				
				return Logger.LogReturn( "Cannot push game state: Loading content failed.", false, LogType.Error );
			}

			m_states.Push( state );
			return true;
		}
		/// <summary>
		///   Disposes of and pops the current state off the manager.
		/// </summary>
		/// <returns></returns>
		public bool Pop()
		{
			if( Empty )
				return false;

			m_states.Peek().UnsubscribeEvents();
			m_states.Peek().Manager = null;
			m_states.Pop().Dispose();

			if( !Empty )
			{
				m_states.Peek().SubscribeEvents();
				m_states.Peek().OnRestore();
			}

			return true;
		}
		/// <summary>
		///   Disposes of all managed game states and optionally switches to a new state.
		/// </summary>
		/// <param name="state">
		///   The state to switch to (or null to dispose and clear state stack).
		/// </param>
		/// <returns>
		///   True on success or false on failure.
		/// </returns>
		public bool Reset( IGameState state = null )
		{
			while( !Empty )
				Pop();

			return state is null || Push( state );
		}

		/// <summary>
		///   Updates the current game state.
		/// </summary>
		/// <param name="dt">
		///   Delta time.
		/// </param>
		public void Update( float dt )
		{
			if( !Empty )
				m_states.Peek().Update( dt );
		}
		/// <summary>
		///   Draws the current state to the render target.
		/// </summary>
		/// <param name="target">
		///   Render target.
		/// </param>
		/// <param name="states">
		///   Render states.
		/// </param>
		public void Draw( RenderTarget target, RenderStates states )
		{
			if( !Empty )
				m_states.Peek().Draw( target, states );
		}

		/// <summary>
		///   Called when an attempt is made to cloe the game window. Return true to allow the
		///   window to close, or false to deny it.
		/// </summary>
		/// <param name="sender">
		///   Event sender.
		/// </param>
		/// <param name="e">
		///   Event arguments.
		/// </param>
		/// <returns>
		///   True if the game window should close, or false to keep it open.
		/// </returns>
		public bool OnCloseRequest( object sender, EventArgs e )
		{
			if( !Empty )
				return m_states.Peek().OnCloseRequest( sender, e );

			return true;
		}

		/// <summary>
		///   Disposes of all game states.
		/// </summary>
		public void Dispose()
		{
			Reset();
			m_states.Clear();
		}

		private readonly Stack<IGameState> m_states;
	}
}
