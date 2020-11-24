////////////////////////////////////////////////////////////////////////////////
// BaseGame.cs 
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
using System.IO;
using SFML.Graphics;
using SFML.Window;

using SharpLogger;
using SharpSerial;

namespace SharpGame
{
	/// <summary>
	///   Base class for games.
	/// </summary>
	public class GameWindow : IDisposable
	{
		/// <summary>
		///   Path to save/load window settings.
		/// </summary>
		public const string SettingsPath = "window.xml";

		/// <summary>
		///   The title of the game.
		/// </summary>
		public virtual string GameTitle
		{
			get { return "New GameWindow"; }
		}

		/// <summary>
		///   Constructor.
		/// </summary>
		public GameWindow()
		{
			Running  = false;
			Settings = null;
			Window   = null;
			Manager  = null;
		}
		/// <summary>
		///   Constructor settings the initial window settings.
		/// </summary>
		/// <param name="settings">
		///   Window settings, this is corrected on assignment.
		/// </param>
		public GameWindow( WindowSettings settings )
		{
			Running  = false;
			Settings = settings?.AsValid() ?? new WindowSettings();
			Window   = null;
			Manager  = null;
		}

		/// <summary>
		///   If the game should be running.
		/// </summary>
		public bool Running
		{
			get; private set;
		}
		/// <summary>
		///   Game exit code.
		/// </summary>
		public int ExitCode
		{
			get; private set;
		}

		/// <summary>
		///   Window settings.
		/// </summary>
		public WindowSettings Settings
		{
			get; private set;
		}
		/// <summary>
		///   Render window.
		/// </summary>
		public RenderWindow Window
		{
			get; private set;
		}
		/// <summary>
		///   State manager.
		/// </summary>
		public StateManager Manager
		{
			get; private set;
		}

		/// <summary>
		///   Runs the game.
		/// </summary>
		/// <param name="state">
		///   The initial game state.
		/// </param>
		/// <returns>
		///   Zero on success or a negative error code on failure.
		/// </returns>
		public int Run( IGameState state )
		{
			if( state == null )
				return -1;

			int result = 0;
			Running = Initialise();

			if( !Running )
				result = -2;
			else if( !LoadContent( state ) )
			{
				result = -3;
				Running = false;
			}

			using( Timestep timer = new Timestep( Settings.TargetFps ) )
			{
				while( Running && Window.IsOpen )
				{
					timer.BeginFrame();
					Window.DispatchEvents();

					if( timer.IsTimeToUpdate )
					{
						Update( timer.DeltaTime );
						Draw();

						timer.EndFrame();
					}
				}
			}

			Dispose();
			return result;
		}
		/// <summary>
		///   Issues an exit request.
		/// </summary>
		/// <param name="exitcode">
		///   Exit code.
		/// </param>
		public void Exit( int exitcode = 0 )
		{
			ExitCode = exitcode;
			Running  = false;
		}

		/// <summary>
		///   Called after initialisation, use this to initialise any additional libraries and
		///   prepare to run the game.
		/// </summary>
		/// <returns>
		///   True on success, false on failure.
		/// </returns>
		protected virtual bool OnInit()
		{
			return true;
		}
		/// <summary>
		///   Called after the initial game state is loaded, use this to load any additional 
		///   content that is not tied to the game state.
		/// </summary>
		/// <returns>
		///   True on success, false on failure.
		/// </returns>
		protected virtual bool OnLoad()
		{
			return true;
		}

		/// <summary>
		///   Called when the game window gains focus.
		/// </summary>
		/// <param name="sender">
		///   Event sender.
		/// </param>
		/// <param name="e">
		///   Event arguments.
		/// </param>
		protected virtual void OnGainFocus( object sender, EventArgs e )
		{ }
		/// <summary>
		///   Called when the game window loses focus.
		/// </summary>
		/// <param name="sender">
		///   Event sender.
		/// </param>
		/// <param name="e">
		///   Event arguments.
		/// </param>
		protected virtual void OnLoseFocus( object sender, EventArgs e )
		{ }
		/// <summary>
		///   Called when the game window gets a close request.
		/// </summary>
		/// <param name="sender">
		///   Event sender.
		/// </param>
		/// <param name="e">
		///   Event arguments.
		/// </param>
		protected virtual void OnClose( object sender, EventArgs e )
		{
			Window?.Close();
		}

		/// <summary>
		///   Called after <see cref="Update(float)"/>, use this to update any additional logic
		///   that is not tied to the game state.
		/// </summary>
		/// <param name="dt">
		///   Delta time.
		/// </param>
		protected virtual void OnUpdate( float dt )
		{ }
		/// <summary>
		///   Called after <see cref="Draw()"/>, use this to draw any additional content on top of
		///   the game state.
		/// </summary>
		protected virtual void OnDraw()
		{ }
		/// <summary>
		///   Called before the game window is disposed of, use this to dispose of any additional
		///   content not tied to the game state.
		/// </summary>
		protected virtual void OnDispose()
		{ }

		/// <summary>
		///   Initialises the game.
		/// </summary>
		/// <returns>
		///   True on success, false on failure.
		/// </returns>
		private bool Initialise()
		{
			// Load/Save WindowSettings.
			if( Settings == null )
				Settings = new WindowSettings();

			Settings.MakeValid();

			if( !File.Exists( SettingsPath ) )
			{
				if( !XmlLoadable.ToFile( Settings, SettingsPath, true ) )
					return Logger.LogReturn( "Failed saving default window settings to file.", false, LogType.Error );
			}
			else
			{
				Settings = XmlLoadable.FromFile<WindowSettings>( SettingsPath );

				if( Settings == null )
					return Logger.LogReturn( "Failed loading window settings from file although it exists.", false, LogType.Error );
			}

			// Set up window.
			if( Window != null )
				Window.Dispose();

			Window = new RenderWindow( new VideoMode( Settings.Width, Settings.Height ), GameTitle, Settings.Fullscreen ? Styles.Fullscreen : Styles.Close );

			if( Window == null || !Window.IsOpen )
				return Logger.LogReturn( "Failed creating render window.", false, LogType.Error );

			Window.Closed      += OnClose;
			Window.GainedFocus += OnGainFocus;
			Window.LostFocus   += OnLoseFocus;

			Manager = new StateManager( this );
			return OnInit();
		}
		/// <summary>
		///   Loads extra content and the given game state ready for the game to run.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		private bool LoadContent( IGameState state )
		{
			return Manager.Reset( state ) && OnLoad();
		}
		/// <summary>
		///   Updates game logic; called every frame before drawing.
		/// </summary>
		/// <param name="dt">
		///   Delta time.
		/// </param>
		private void Update( float dt )
		{
			Manager.Update( dt );
			OnUpdate( dt );
		}
		/// <summary>
		///   Draws the game content to the render window; called every frame after updating.
		/// </summary>
		private void Draw()
		{
			Window.Clear();

			Window.Draw( Manager );
			OnDraw();

			Window.Display();
		}
		/// <summary>
		///   Disposes of all game content, ready to be destroyed.
		/// </summary>
		public void Dispose()
		{
			OnDispose();

			if( !XmlLoadable.ToFile( Settings, SettingsPath, true ) )
				Logger.Log( "Unable to save window settings to file.", LogType.Error );

			if( Manager != null )
			{
				Manager.Dispose();
				Manager = null;
			}
			if( Window != null )
			{
				Window.Dispose();
				Window = null;
			}
		}
	}
}
