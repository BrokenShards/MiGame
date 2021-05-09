////////////////////////////////////////////////////////////////////////////////
// Timestep.cs 
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
using SFML.System;

namespace MiGame
{
	/// <summary>
	///   Keeps time in the game.
	/// </summary>
	public sealed class Timestep : IDisposable
	{
		/// <summary>
		///   Constructs the instance, optionally setting the target frames per second.
		/// </summary>
		/// <param name="fps">
		///   The target frames per second.
		/// </param>
		public Timestep( float fps = 60.0f )
		{
			m_overhang     = Time.Zero;
			m_deltaTime    = Time.Zero;
			IsTimeToUpdate = false;
			TargetFPS      = fps;
			m_clock        = new Clock();
		}

		/// <summary>
		///   Constructs the instance as a copy of another.
		/// </summary>
		/// <param name="t">
		///   The instance to copy.
		/// </param>
		public Timestep( Timestep t )
		{
			if( t is null )
				throw new ArgumentNullException( nameof( t ) );

			m_overhang     = t.m_overhang;
			m_deltaTime    = t.m_deltaTime;
			IsTimeToUpdate = t.IsTimeToUpdate;
			TargetFPS      = t.TargetFPS;
			m_clock        = new Clock();
		}

		/// <summary>
		///   The target frames per second.
		/// </summary>
		public float TargetFPS
		{
			get { return m_targetFps; }
			set
			{
				if( value <= 0.0f )
					throw new ArgumentException( "Target FPS cannot be less than or equal to zero." );

				m_targetFps    = value;
				m_timePerFrame = Time.FromSeconds( 1.0f / m_targetFps );
			}
		}
		/// <summary>
		///   The delta time between frames in seconds.
		/// </summary>
		public float DeltaTime
		{
			get { return m_deltaTime.AsSeconds(); }
		}
		/// <summary>
		///   If it is time to update/draw.
		/// </summary>
		public bool IsTimeToUpdate { get; private set; }

		/// <summary>
		///   To be called at the beginning of the frame.
		/// </summary>
		public void BeginFrame()
		{
			m_deltaTime = m_clock.ElapsedTime + m_overhang;
			m_overhang  = Time.Zero;

			if( m_deltaTime >= m_timePerFrame )
			{
				m_overhang     = m_deltaTime - m_timePerFrame;
				IsTimeToUpdate = true;
			}
		}
		/// <summary>
		///   To be called at the end of a frame.
		/// </summary>
		public void EndFrame()
		{
			IsTimeToUpdate = false;
			m_clock.Restart();
		}
		/// <summary>
		///   Disposes of the internal clock resources.
		/// </summary>
		public void Dispose()
		{
			m_clock?.Dispose();
		}

		private Time  m_timePerFrame,
					  m_overhang,
					  m_deltaTime;
		private float m_targetFps;
		private readonly Clock m_clock;
	}
}
