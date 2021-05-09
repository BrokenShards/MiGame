////////////////////////////////////////////////////////////////////////////////
// BaseGame.cs 
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
using System.IO;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

using MiCore;
using MiInput;

using Action = MiInput.Action;

namespace MiGame
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
			get { return m_title; }
			set 
			{
				m_title = string.IsNullOrWhiteSpace( value ) ? string.Empty : value;
				Window?.SetTitle( m_title );
			}
		}

		/// <summary>
		///   Constructor.
		/// </summary>
		public GameWindow()
		{
			Running    = false;
			Settings   = new WindowSettings();
			ClearColor = Color.Black;
			Window     = null;
			Manager    = null;
			RunAgain   = false;

			LoadWindowSettings = true;
			SaveWindowSettings = true;
			LoadInputSettings  = true;
			SaveInputSettings  = true;
		}
		/// <summary>
		///   Constructor settings the initial window settings.
		/// </summary>
		/// <param name="settings">
		///   Window settings, this is corrected on assignment.
		/// </param>
		public GameWindow( WindowSettings settings )
		{
			Running    = false;
			Settings   = settings?.AsValid() ?? new WindowSettings();
			ClearColor = Color.Black;
			Window     = null;
			Manager    = null;
			RunAgain   = false;
			
			LoadWindowSettings = false;
			SaveWindowSettings = true;
			LoadInputSettings  = true;
			SaveInputSettings  = true;
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
		///   Window clear color.
		/// </summary>
		public Color ClearColor
		{
			get; set;
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
		///   If window settings should be loaded from file on init.
		/// </summary>
		public bool LoadWindowSettings
		{
			get; set;
		}
		/// <summary>
		///   If window settings should be saved to file.
		/// </summary>
		public bool SaveWindowSettings
		{
			get; set;
		}

		/// <summary>
		///   If input settings should be loaded from file on init.
		/// </summary>
		public bool LoadInputSettings
		{
			get; set;
		}
		/// <summary>
		///   If input settings should be saved to file.
		/// </summary>
		public bool SaveInputSettings
		{
			get; set;
		}

		/// <summary>
		///   If the game should run again when on closed. Set to false before consecutuve run.
		/// </summary>
		public bool RunAgain
		{
			get; set;
		}

		/// <summary>
		///   Changes the current window settings, optionally restarting the game for the changes to
		///   take effect. Has no effect if <see cref="SaveWindowSettings"/> is false.
		/// </summary>
		/// <remarks>
		///   Changes will not be loaded on restart if <see cref="LoadWindowSettings"/> is false.
		///   Changes will not be saved if <see cref="SaveWindowSettings"/> is false as thus will
		///   not be applied unless <see cref="LoadWindowSettings"/> is also false.
		/// </remarks>
		/// <param name="settings">
		///   Window settings.
		/// </param>
		/// <param name="restart">
		///   If game should close and restart to apply new changes.
		/// </param>
		/// <returns>
		///   True if settings were valid and were assigned and written to file, otherwise false.
		/// </returns>
		public bool ChangeSettings( WindowSettings settings, bool restart = false )
		{
			if( settings is null || !settings.IsValid )
				return false;

			Settings = settings;

			if( SaveWindowSettings && !XmlLoadable.ToFile( Settings, SettingsPath, true ) )
				return Logger.LogReturn( "Unable to save newly set window settings to file.", false, LogType.Error );

			if( restart )
			{
				RunAgain = true;
				Exit();
			}

			return true;
		}

		/// <summary>
		///   Runs the game.
		/// </summary>
		/// <typeparam name="T">
		///   The game state type to start with.
		/// </typeparam>
		/// <returns>
		///   Exit code.
		/// </returns>
		public ExitCode Run<T>() where T : class, IGameState, new()
		{
			if( Running )
			{
				ExitCode = ExitCode.UnexpectedRun;
				Running  = false;
			}
			else
				Running = Initialise() && LoadContent( new T() );

			try
			{
				using Timestep timer = new( Settings.TargetFps );

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
			catch( Exception e )
			{
				ExitCode = Logger.LogReturn( $"Unexpected exception: { e.Message }", ExitCode.UnexpectedFail, LogType.Error );
			}

			Dispose();

			if( RunAgain )
			{
				RunAgain = false;
				return Run<T>();
			}

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

			Running = false;
		}

		/// <summary>
		///   Disposes of all game content, ready to be destroyed.
		/// </summary>
		public void Dispose()
		{
			Running = false;
			OnDispose();

			if( SaveInputSettings )
				if( !Input.Manager.SaveToFile() )
					Logger.Log( "Unable to save input settings to file.", LogType.Error );

			if( SaveWindowSettings )
				if( !XmlLoadable.ToFile( Settings, SettingsPath, true ) )
					Logger.Log( "Unable to save window settings to file.", LogType.Error );

			if( Manager is not null )
			{
				Manager.Dispose();
				Manager = null;
			}
			if( Window is not null )
			{
				Window.Dispose();
				Window = null;
			}

			GC.SuppressFinalize( this );
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

		/// <summary>
		///   Sets up default input mapping.
		/// </summary>
		/// <returns>
		///   True on success, false on failure.
		/// </returns>
		protected virtual bool MapDefaultActions()
		{
			Action decision = new( "Decision",
							  new InputMap( InputDevice.Keyboard, InputType.Button, "Enter", "Escape" ),
							  new InputMap( InputDevice.Keyboard, InputType.Button, "Space", "Backspace" ),
							  new InputMap( InputDevice.Joystick, InputType.Button, "A", "B" ) ),
				   submit   = new( "Submit",
							  new InputMap( InputDevice.Keyboard, InputType.Button, "Enter" ),
							  new InputMap( InputDevice.Keyboard, InputType.Button, "Space" ),
							  new InputMap( InputDevice.Joystick, InputType.Button, "A" ) ),
				   cancel   = new( "Cancel",
							  new InputMap( InputDevice.Keyboard, InputType.Button, "Escape" ),
							  new InputMap( InputDevice.Keyboard, InputType.Button, "Backspace" ),
							  new InputMap( InputDevice.Joystick, InputType.Button, "B" ) ),
				   horizont = new( "Horizontal",
							  new InputMap( InputDevice.Keyboard, InputType.Button, "D",         "A" ),
							  new InputMap( InputDevice.Keyboard, InputType.Button, "Right",     "Left" ),
							  new InputMap( InputDevice.Joystick, InputType.Button, "DPadRight", "DPadLeft" ),
							  new InputMap( InputDevice.Joystick, InputType.Axis,   "LeftStickX" ) ),
				   vertical = new( "Vertical",
							  new InputMap( InputDevice.Keyboard, InputType.Button, "W",      "S" ),
							  new InputMap( InputDevice.Keyboard, InputType.Button, "Up",     "Down" ),
							  new InputMap( InputDevice.Joystick, InputType.Button, "DPadUp", "DPadDown" ),
							  new InputMap( InputDevice.Joystick, InputType.Axis,   "LeftStickY" ) );

				if( !Input.Manager.Actions.Add( decision ) || !Input.Manager.Actions.Add( submit ) ||
				    !Input.Manager.Actions.Add( cancel ) ||	!Input.Manager.Actions.Add( horizont ) ||
					!Input.Manager.Actions.Add( vertical ) )
					return false;

			return true;
		}

		private bool Initialise()
		{
			// Load input settings.
			if( LoadInputSettings )
			{
				if( !Input.Manager.LoadFromFile() )
				{
					if( !MapDefaultActions() )
						return Logger.LogReturn( "Failed assigning default input actions.", false, LogType.Error );

					if( !Input.Manager.SaveToFile() )
						Logger.Log( "Failed saving default inputs. Any modifications to the assigned inputs will not be saved.", LogType.Warning );
				}
			}
			else if( !MapDefaultActions() )
				return Logger.LogReturn( "Failed assigning default input actions.", false, LogType.Error );

			if( Settings is null )
				Settings = new WindowSettings();

			// Load/Save WindowSettings.
			if( LoadWindowSettings )
			{
				if( File.Exists( SettingsPath ) )
				{
					Settings = XmlLoadable.FromFile<WindowSettings>( SettingsPath );

					if( Settings is null )
					{
						ExitCode = ExitCode.InitFail;
						return Logger.LogReturn( "Failed loading window settings from file although it exists.", false, LogType.Error );
					}
				}
			}

			Settings.MakeValid();

			if( SaveWindowSettings )
			{
				if( !XmlLoadable.ToFile( Settings, SettingsPath, true ) )
				{
					ExitCode = ExitCode.InitFail;
					return Logger.LogReturn( "Failed saving window settings to file.", false, LogType.Error );
				}
			}

			// Set up window.
			if( Window is not null )
				Window.Dispose();

			Styles wm = Settings.WindowMode is WindowMode.Bordered   ? Styles.Titlebar : 
			          ( Settings.WindowMode is WindowMode.Fullscreen ? Styles.Fullscreen : Styles.None );

			if( wm is Styles.Titlebar )
			{
				if( Settings.Close )
					wm |= Styles.Close;
				if( Settings.Resizable )
					wm |= Styles.Resize;
			}

			Window = new RenderWindow( new VideoMode( Settings.Width, Settings.Height ), GameTitle, wm );

			if( Window is null || !Window.IsOpen )
			{
				ExitCode = ExitCode.InitFail;
				return Logger.LogReturn( "Failed creating render window.", false, LogType.Error );
			}

			Window.Closed += OnClose;
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
			Input.Manager.Update();

			PreUpdate( dt );
			Manager.Update( dt );
			PostUpdate( dt );
		}
		private void Draw()
		{
			Window.Clear( ClearColor );

			PreDraw();
			Window.Draw( Manager );
			PostDraw();

			Window.Display();
		}

		private void OnClose( object sender, EventArgs e )
		{
			if( Manager is not null && !Manager.OnCloseRequest( sender, e ) )
				return;

			Window?.Close();
		}

		private string m_title;
	}
}
