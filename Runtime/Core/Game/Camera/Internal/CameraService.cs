﻿using System.Linq;
using Espionage.Engine.Tripods;
using UnityEngine;

namespace Espionage.Engine.Services
{
	[Order( -5 )]
	internal class CameraService : Service
	{
		public override void OnReady()
		{
			if ( !Application.isPlaying )
			{
				return;
			}

			var obj = new GameObject( "Main Camera" ).MoveTo( Engine.Scene );

			// Setup Camera
			_camera = obj.AddComponent<CameraController>();

			Callback.Run( "camera.created", _camera );
		}

		// Frame

		private CameraController _camera;

		private ITripod.Setup _lastSetup = new()
		{
			Rotation = Quaternion.identity,
			FieldOfView = 60,
			Position = Vector3.zero
		};

		public void OnCameraUpdate()
		{
			if ( Engine.Game == null || !Application.isPlaying )
			{
				return;
			}

			if ( Input.GetKeyDown( KeyCode.F1 ) )
			{
				Local.Client.Tripod = Local.Client.Tripod == null ? new DevTripod() : null;
			}

			// Build the camSetup, from game.
			_lastSetup = Engine.Game.BuildCamera( _lastSetup );

			// Finalise
			_camera.Finalise( _lastSetup );

			// Set the viewer to null, so its cleared every frame.
			_lastSetup.Viewer = null;
		}
	}
}