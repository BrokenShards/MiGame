////////////////////////////////////////////////////////////////////////////////
// StateManager.cs 
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
using System.Collections.Generic;

using SFML.Graphics;
using SharpLogger;

namespace SharpGame
{
	/// <summary>
	///   Manages running game states.
	/// </summary>
	public class StateManager : Drawable, IDisposable
	{
		/// <summary>
		///   Constructor.
		/// </summary>
		public StateManager( GameWindow game )
		{
			m_states = new Stack<IGameState>();
			Game     = game;
		}

		/// <summary>
		///   The game window.
		/// </summary>
		public GameWindow Game
		{
			get; private set;
		}

		/// <summary>
		///   If the manager contains no states.
		/// </summary>
		public bool Empty
		{
			get { return Count == 0; }
		}
		/// <summary>
		///   The amount of states the manager currently contains.
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
			if( state == null )
				return Logger.LogReturn( "Cannot push null game state.", false, LogType.Error );

			state.Manager = this;

			if( !Empty )
			{
				if( m_states.Peek().Storable )
					m_states.Peek().OnStore();
				else
					m_states.Pop().Dispose();
			}

			if( !state.LoadContent() )
			{
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

			m_states.Peek().Manager = null;
			m_states.Pop().Dispose();

			if( !Empty )
				m_states.Peek().onRestore();

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

			return state == null || Push( state );
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
		///   Disposes of all game states.
		/// </summary>
		public void Dispose()
		{
			Reset();
			m_states = null;
			Game = null;
		}

		private Stack<IGameState> m_states;
	}
}
