////////////////////////////////////////////////////////////////////////////////
// GameState.cs 
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
using SFML.Graphics;
using SFML.Window;

namespace MiGame
{
    /// <summary>
    ///   Base interface for game states.
    /// </summary>
    public interface IGameState : Drawable, IDisposable
	{
        /// <summary>
        ///   If the state can be stored in the background for later use.
        /// </summary>
        bool Storable { get; }

		/// <summary>
		///   The state manager that is managing this state.
		/// </summary>
		StateManager Manager { get; set; }

        /// <summary>
        ///   Initialises the game state and loads any content that is needed.
        /// </summary>
        /// <returns>
        ///   True if the game state was initialised and all content was loaded successfully,
        ///   otherwise false.
        /// </returns>
        bool LoadContent();
        /// <summary>
        ///   Updates the logic of the game state. Called every frame.
        /// </summary>
        /// <param name="dt">
		///   Delta time.
		/// </param>
        void Update( float dt );

		/// <summary>
		///   Called when a storable state is stored.
		/// </summary>
		void OnStore();
		/// <summary>
		///   Called when a storable state is restored.
		/// </summary>
		void OnRestore();

		/// <summary>
		///   Subscribes to window events.
		/// </summary>
		void SubscribeEvents();
		/// <summary>
		///   Unsubscribes to window events.
		/// </summary>
		void UnsubscribeEvents();

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
		bool OnCloseRequest( object sender, EventArgs e );
	}

	/// <summary>
	///   Base class for game states.
	/// </summary>
	public abstract class GameState : IGameState
	{
		/// <summary>
		///   Constructor.
		/// </summary>
		public GameState()
		{
			Manager = null;
		}
		/// <summary>
		///   Constructor that sets the state manager.
		/// </summary>
		/// <param name="manager">
		///   The state manager.
		/// </param>
		public GameState( StateManager manager )
		{
			Manager = manager;
		}

		/// <summary>
		///   If the state can be stored in the background for later use.
		/// </summary>
		public virtual bool Storable
		{
			get { return false; }
		}

		/// <summary>
		///   The state manager that is managing this state.
		/// </summary>
		public StateManager Manager
		{
			get; set;
		}

		/// <summary>
		///   Initialises the game state and loads any resources that are needed.
		/// </summary>
		/// <returns>
		///   True if the game state was initialised and all resources were loaded successfully,
		///   otherwise false.
		/// </returns>
		public abstract bool LoadContent();
		/// <summary>
		///   Updates the logic of the game state. Called every frame.
		/// </summary>
		/// <param name="dt">
		///   Delta time.
		/// </param>
		public abstract void Update( float dt );
		/// <summary>
		///   Draws the state to the render target.
		/// </summary>
		/// <param name="target">
		///   Render target.
		/// </param>
		/// <param name="states">
		///   Render states.
		/// </param>
		public abstract void Draw( RenderTarget target, RenderStates states );
		/// <summary>
		///   Disposes of loaded resources and cleans up, ready for the state to be destroyed.
		/// </summary>
		public abstract void Dispose();

		/// <summary>
		///   Subscribes to window events.
		/// </summary>
		public virtual void SubscribeEvents()
		{ }
		/// <summary>
		///   Unsubscribes to window events.
		/// </summary>
		public virtual void UnsubscribeEvents()
		{ }

		/// <summary>
		///   Called when a storable state is stored.
		/// </summary>
		public virtual void OnStore()
		{ }
		/// <summary>
		///   Called when a storable state is restored.
		/// </summary>
		public virtual void OnRestore()
		{ }
		

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
		public virtual bool OnCloseRequest( object sender, EventArgs e )
		{
			return true;
		}
	}
}
