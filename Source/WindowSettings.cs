////////////////////////////////////////////////////////////////////////////////
// WindowSettings.cs 
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
using System.Text;
using System.Xml;
using SFML.Window;
using SharpLogger;
using SharpSerial;

namespace SharpGame
{
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
			Width      = 800;
			Height     = 600;
			Fullscreen = false;
			TargetFps  = 60.0f;
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
			if( ws == null )
				throw new ArgumentNullException();

			Width      = ws.Width;
			Height     = ws.Height;
			Fullscreen = ws.Fullscreen;
			TargetFps  = ws.TargetFps;
		}

		/// <summary>
		///   Window width.
		/// </summary>
		public uint Width
		{
			get { return m_width; }
			set { m_width = value == 0 ? 1 : value; }
		}
		/// <summary>
		///   Window height.
		/// </summary>
		public uint Height 
		{
			get { return m_height; }
			set { m_height = value == 0 ? 1 : value; }
		}
		/// <summary>
		///   If window should be fullscreen or windowed.
		/// </summary>
		public bool Fullscreen
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
		///   Checks if the current settings are valid.
		/// </summary>
		public bool IsValid
		{
			get
			{
				if( Fullscreen )
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

			if( Fullscreen )
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
			WindowSettings w = new WindowSettings( this );
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
			return Width      == other.Width &&
			       Height     == other.Height &&
				   Fullscreen == other.Fullscreen &&
				   TargetFps  == other.TargetFps;
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
			if( element == null )
				return Logger.LogReturn( "Cannot load window settings from null xml element.", false, LogType.Error );
			if( element.Name.ToLower() != "window" )
				return Logger.LogReturn( "Unable to load window settings: Element name must be 'window'.", false, LogType.Error );

			try
			{
				Width      = uint.Parse( element.Attributes[ "width" ]?.Value );
				Height     = uint.Parse( element.Attributes[ "height" ]?.Value );
				Fullscreen = bool.Parse( element.Attributes[ "fullscreen" ]?.Value );
				TargetFps  = float.Parse( element.Attributes[ "target_fps" ]?.Value );
			}
			catch( Exception e )
			{
				return Logger.LogReturn( "Unable to load window settings: " + e.Message + ".", false, LogType.Error );
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
			StringBuilder sb = new StringBuilder();

			sb.Append( "<window width=\"" );
			sb.Append( Width );
			sb.AppendLine( "\"" );

			sb.Append( "        height=\"" );
			sb.Append( Height );
			sb.AppendLine( "\"" );

			sb.Append( "        fullscreen=\"" );
			sb.Append( Fullscreen );
			sb.AppendLine( "\"" );

			sb.Append( "        target_fps=\"" );
			sb.Append( TargetFps );
			sb.Append( "\" />" );

			return sb.ToString();
		}

		private uint  m_width, m_height;
		private float m_fps;
	}
}
