////////////////////////////////////////////////////////////////////////////////
// WindowSettings.cs 
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
using System.Text;
using System.Xml;

using SFML.Window;
using SFML.System;

using MiCore;

namespace MiGame
{
	/// <summary>
	///   Possible window modes.
	/// </summary>
	public enum WindowMode
	{
		/// <summary>
		///   Windowed with a titlebar.
		/// </summary>
		Bordered,
		/// <summary>
		///   Windowed with no titlebar or borders.
		/// </summary>
		Borderless,
		/// <summary>
		///   Fullscreen.
		/// </summary>
		Fullscreen
	}

	/// <summary>
	///   Contains settings used to construct a window.
	/// </summary>
	public class WindowSettings : XmlLoadable, IEquatable<WindowSettings>
	{
		/// <summary>
		///  Constructor.
		/// </summary>
		public WindowSettings()
		{
			Size       = new Vector2u( 800, 600 );
			WindowMode = 0;
			TargetFps  = 60.0f;
			Close      = true;
			Resizable  = false;
		}
		/// <summary>
		///   Copy constructor.
		/// </summary>
		/// <param name="ws">
		///   The object to copy.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///   If <paramref name="ws"/> is null.
		/// </exception>
		public WindowSettings( WindowSettings ws )
		{
			if( ws is null )
				throw new ArgumentNullException( nameof( ws ) );

			Size       = ws.Size;
			WindowMode = ws.WindowMode;
			Close      = ws.Close;
			Resizable  = ws.Resizable;
			TargetFps  = ws.TargetFps;
		}
		/// <summary>
		///   Constructor assigning window size, mode flags and clear color, along with target fps.
		/// </summary>
		/// <param name="size">
		///   Window size.
		/// </param>
		/// <param name="wm">
		///   Window mode
		/// </param>
		/// <param name="close">
		///   If window should be created with a close button.
		/// </param>
		/// <param name="resize">
		///   If window should be resizable.
		/// </param>
		/// <param name="fps">
		///   Target FPS.
		/// </param>
		public WindowSettings( Vector2u size, WindowMode wm = 0, bool close = true, bool resize = false, float fps = 60.0f )
		{
			Size       = size;
			WindowMode = wm;
			Close      = close;
			Resizable  = resize;
			TargetFps  = fps;
		}

		/// <summary>
		///   Window size.
		/// </summary>
		public Vector2u Size
		{
			get { return m_size; }
			set
			{
				if( value.X is 0 )
					value.X = 1;
				if( value.Y is 0 )
					value.Y = 1;

				m_size = value;
			}
		}

		/// <summary>
		///   Window width.
		/// </summary>
		public uint Width
		{
			get { return Size.X; }
			set { Size = new Vector2u( value, Size.Y ); }
		}
		/// <summary>
		///   Window height.
		/// </summary>
		public uint Height 
		{
			get { return Size.Y; }
			set { Size = new Vector2u( Size.X, value ); }
		}

		/// <summary>
		///   If window should be fullscreen or windowed.
		/// </summary>
		public WindowMode WindowMode
		{
			get; set;
		}
		/// <summary>
		///   Target frames per second.
		/// </summary>
		public float TargetFps
		{
			get { return m_fps; }
			set { m_fps = value <= 0.0f ? 1.0f : value; }
		}

		/// <summary>
		///   If a bordered window should be created with a close button.
		/// </summary>
		public bool Close
		{
			get; set;
		}
		/// <summary>
		///   If the window should be resizable.
		/// </summary>
		public bool Resizable
		{
			get; set;
		}

		/// <summary>
		///   Checks if the current settings are valid.
		/// </summary>
		public bool IsValid
		{
			get
			{
				if( WindowMode is WindowMode.Fullscreen )
				{
					if( !new VideoMode( Width, Height ).IsValid() )
						return false;
				}
				else
				{
					VideoMode desk = VideoMode.DesktopMode;

					if( Width > desk.Width || Height > desk.Height )
						return false;
				}

				return true;
			}
		}

		/// <summary>
		///   Corrects invalid settings.
		/// </summary>
		public void MakeValid()
		{
			VideoMode desk = VideoMode.DesktopMode;

			if( WindowMode is WindowMode.Fullscreen )
			{
				if( !new VideoMode( Width, Height ).IsValid() )
				{
					Width  = desk.Width;
					Height = desk.Height;
				}
			}
			else
			{
				if( Width > desk.Width )
				{
					float ratio = Height / (float)Width;

					Width  = desk.Width;
					Height = (uint)( Width * ratio );
				}
				if( Height > desk.Height )
				{
					float ratio = Width / (float)Height;

					Height = desk.Height;
					Width = (uint)( Height * ratio );
				}
			}
		}
		/// <summary>
		///   Returns a new settings object with corrected values.
		/// </summary>
		/// <returns>
		///   Returns a new settings object with corrected values.
		/// </returns>
		public WindowSettings AsValid()
		{
			WindowSettings w = new( this );
			w.MakeValid();
			return w;
		}

		/// <summary>
		///   If this object is equal to another object.
		/// </summary>
		/// <param name="other">
		///   The object to check against.
		/// </param>
		/// <returns>
		///   True if this object is equal to other and false if it is not.
		/// </returns>
		public bool Equals( WindowSettings other )
		{
			return other is not null &&
			       Size.Equals( other.Size ) &&
				   WindowMode == other.WindowMode &&
				   Close      == other.Close &&
				   Resizable  == other.Resizable &&
				   TargetFps  == other.TargetFps;
		}
		/// <summary>
		///   If this object has the same values of the other object.
		/// </summary>
		/// <param name="obj">
		///   The other object to check against.
		/// </param>
		/// <returns>
		///   True if both objects are concidered equal and false if they are not.
		/// </returns>
		public override bool Equals( object obj )
		{
			return Equals( obj as WindowSettings );
		}

		/// <summary>
		///   Serves as the default hash function.
		/// </summary>
		/// <returns>
		///   A hash code for the current object.
		/// </returns>
		public override int GetHashCode()
		{
			return HashCode.Combine( Size, WindowMode, Close, Resizable, TargetFps );
		}

		/// <summary>
		///   Attempts to load the object from the xml element.
		/// </summary>
		/// <param name="element">
		///   The xml element.
		/// </param>
		/// <returns>
		///   True if loaded successfully and false otherwise.
		/// </returns>
		public override bool LoadFromXml( XmlElement element )
		{
			if( element is null )
				return Logger.LogReturn( "Cannot load WindowSettings from null xml element.", false, LogType.Error );

			WindowMode = 0;
			TargetFps  = 60.0f;
			Close      = true;
			Resizable  = false;

			XmlElement sizeele = element[ nameof( Size ) ];
			Vector2u?  size    = Xml.ToVec2u( sizeele );

			if( sizeele is null  )
				return Logger.LogReturn( "Failed loading WindowSettings: Size element missing.", false, LogType.Error );
			if( !size.HasValue )
				return Logger.LogReturn( "Failed loading WindowSettings: Parsing Size failed.", false, LogType.Error );

			Size = size.Value;

			try
			{
				if( element.HasAttribute( nameof( WindowMode ) ) )
					WindowMode = (WindowMode)Enum.Parse( typeof( WindowMode ), element.GetAttribute( nameof( WindowMode ) ), true );
				if( element.HasAttribute( nameof( TargetFps ) ) )
					TargetFps = float.Parse( element.GetAttribute( nameof( TargetFps ) ) );
				if( element.HasAttribute( nameof( Close ) ) )
					Close = bool.Parse( element.GetAttribute( nameof( Close ) ) );
				if( element.HasAttribute( nameof( Resizable ) ) )
					Resizable = bool.Parse( element.GetAttribute( nameof( Resizable ) ) );
			}
			catch( Exception e )
			{
				return Logger.LogReturn( $"Failed loading WindowSettings: { e.Message }", false, LogType.Error );
			}

			return true;
		}

		/// <summary>
		///   Converts the object into an xml string.
		/// </summary>
		/// <returns>
		///   Returns the object as an xml string.
		/// </returns>
		public override string ToString()
		{
			StringBuilder sb = new();

			sb.Append( '<' ).Append( nameof( WindowSettings ) ).Append( ' ' )
				.Append( nameof( WindowMode ) ).Append( "=\"" ).Append( WindowMode.ToString() ).Append( '\"' );
			
			if( !Close )
			{
				sb.AppendLine().Append( "                " )
					.Append( nameof( Close ) ).Append( "=\"" ).Append( Close ).Append( '\"' );
			}
			if( Resizable )
			{
				sb.AppendLine().Append( "                " )
					.Append( nameof( Resizable ) ).Append( "=\"" ).Append( Resizable ).Append( '\"' );
			}
			if( TargetFps is not 60.0f )
			{
				sb.AppendLine().Append( "                " )
					.Append( nameof( TargetFps ) ).Append( "=\"" ).Append( TargetFps ).Append( '\"' );
			}

			return sb.AppendLine( ">" )
				.AppendLine( Xml.ToString( Size, nameof( Size ), 1 ) )
				
				.Append( "</" ).Append( nameof( WindowSettings ) ).Append( '>' )
				.ToString();
		}

		private Vector2u m_size;
		private float    m_fps;
	}
}
