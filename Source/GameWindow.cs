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
	///   Possible game exit codes.
	/// </summary>
	public enum ExitCode
	{
		/// <summary>
		///   If the game ran successfully.
		/// </summary>
		Success =  0,

		/// <summary>
		///   If game initialisation failed.
		/// </summary>
		InitFail = -1,
		/// <summary>
		///   If additional game initialisation failed.
		/// </summary>
		OnInitFail = -2,
		/// <summary>
		///   If additional content loading failed.
		/// </summary>
		OnLoadFail = -3,

		/// <summary>
		///   If trying to load a null game state.
		/// </summary>
		NullStateFail = -4,
		/// <summary>
		///   If loading the game state fails.
		/// </summary>
		StateLoadFail = -5,

		/// <summary>
		///   If an unexpected exception is thrown during execution.
		/// </summary>
		UnexpectedFail = -6,

		/// <summary>
		///   If an attempt was made to run the game while it is already running.
		/// </summary>
		UnexpectedRun = -7
	}

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
		public ExitCode ExitCode
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
		public ExitCode Run( IGameState state )
		{
			if( state == null )
				ExitCode = ExitCode.NullStateFail;
			else if( Running )
			{
				ExitCode = ExitCode.UnexpectedRun;
				Running = false;
			}
			else
				Running = Initialise() && LoadContent( state );

			try
			{
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
			}
			catch( Exception e )
			{
				Logger.Log( "Unexpected exception thrown: \"" + e.Message + "\".", LogType.Error );
				ExitCode = ExitCode.UnexpectedFail;
			}

			Dispose();
			return ExitCode;
		}
		/// <summary>
		///   Issues an exit request.
		/// </summary>
		/// <param name="exitcode">
		///   Exit code or null to leave exit code unchanged.
		/// </param>
		public void Exit( ExitCode? exitcode = null )
		{
			if( exitcode.HasValue )
				ExitCode = exitcode.Value;

			Running  = false;
		}

		/// <summary>
		///   Disposes of all game content, ready to be destroyed.
		/// </summary>
		public void Dispose()
		{
			Running = false;
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
		///   Called before the state manager updates the game state. Put any additional logic that
		///   is required but not tied to the game state in here.
		/// </summary>
		/// <param name="dt">
		///   Delta time.
		/// </param>
		protected virtual void PreUpdate( float dt )
		{ }
		/// <summary>
		///   Called after the state manager updates the game state. Put any additional logic that
		///   not tied to the game state in here.
		/// </summary>
		/// <param name="dt">
		///   Delta time.
		/// </param>
		protected virtual void PostUpdate( float dt )
		{ }
		/// <summary>
		///   Called before the state manager draws the game state. Use this to draw any additional
		///   content underneath the game state.
		/// </summary>
		protected virtual void PreDraw()
		{ }
		/// <summary>
		///   Called after the state manager draws the game state. Use this to draw any additional
		///   content on top of the game state.
		/// </summary>
		protected virtual void PostDraw()
		{ }

		/// <summary>
		///   Called before the game window is disposed of, use this to dispose of any additional
		///   content not tied to the game state.
		/// </summary>
		protected virtual void OnDispose()
		{ }

		private bool Initialise()
		{
			// Load/Save WindowSettings.
			if( Settings == null )
				Settings = new WindowSettings();

			Settings.MakeValid();

			if( !File.Exists( SettingsPath ) )
			{
				if( !XmlLoadable.ToFile( Settings, SettingsPath, true ) )
				{
					ExitCode = ExitCode.InitFail;
					return Logger.LogReturn( "Failed saving default window settings to file.", false, LogType.Error );
				}
			}
			else
			{
				Settings = XmlLoadable.FromFile<WindowSettings>( SettingsPath );

				if( Settings == null )
				{
					ExitCode = ExitCode.InitFail;
					return Logger.LogReturn( "Failed loading window settings from file although it exists.", false, LogType.Error );
				}
			}

			// Set up window.
			if( Window != null )
				Window.Dispose();

			Window = new RenderWindow( new VideoMode( Settings.Width, Settings.Height ), GameTitle, Settings.Fullscreen ? Styles.Fullscreen : Styles.Close );

			if( Window == null || !Window.IsOpen )
			{
				ExitCode = ExitCode.InitFail;
				return Logger.LogReturn( "Failed creating render window.", false, LogType.Error );
			}

			Window.Closed      += OnClose;
			Window.GainedFocus += OnGainFocus;
			Window.LostFocus   += OnLoseFocus;
			Window.TextEntered += OnTextEntered;

			Manager = new StateManager( this );

			if( !OnInit() )
			{
				ExitCode = ExitCode.OnInitFail;
				return false;
			}

			return true;
		}
		private bool LoadContent( IGameState state )
		{
			if( !Manager.Reset( state ) )
			{
				ExitCode = ExitCode.StateLoadFail;
				return false;
			}
			if( !OnLoad() )
			{
				ExitCode = ExitCode.OnLoadFail;
				return false;
			}

			return true;
		}
		private void Update( float dt )
		{
			PreUpdate( dt );
			Manager.Update( dt );
			PostUpdate( dt );
		}
		private void Draw()
		{
			Window.Clear();

			PreDraw();
			Window.Draw( Manager );
			PostDraw();

			Window.Display();
		}

		private void OnGainFocus( object sender, EventArgs e )
		{
			Manager?.OnGainFocus( sender, e );
		}
		private void OnLoseFocus( object sender, EventArgs e )
		{
			Manager?.OnLoseFocus( sender, e );
		}
		private void OnTextEntered( object sender, TextEventArgs e )
		{
			Manager?.OnTextEntered( sender, e );
		}
		private void OnClose( object sender, EventArgs e )
		{
			if( Manager != null && !Manager.OnCloseRequest( sender, e ) )
				return;

			Window?.Close();
		}
	}
}
