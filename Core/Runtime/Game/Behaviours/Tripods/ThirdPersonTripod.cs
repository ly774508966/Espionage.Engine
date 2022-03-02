﻿using UnityEngine;

namespace Espionage.Engine.Cameras
{
	public class ThirdPersonTripod : Tripod
	{
		public override void Activated( ref ITripod.Setup camSetup )
		{
			camSetup.Position = Local.Pawn.EyePos;
			camSetup.Rotation = Local.Pawn.EyeRot;
		}

		protected override void OnBuildTripod( ref ITripod.Setup camSetup )
		{
			if ( Local.Pawn == null )
			{
				return;
			}

			var pawn = Local.Pawn;

			// Set Rot first cause we use it below
			camSetup.Rotation = Quaternion.Slerp( camSetup.Rotation, pawn.EyeRot, smoothing * Time.deltaTime );

			// Do a ray to calculate the distance, so we don't hit shit
			var ray = Physics.Raycast( new Ray( pawn.EyePos, camSetup.Rotation * Vector3.back ), out var hitInfo, distance );
			var relativeOffset = camSetup.Rotation * Vector3.right * offset.x + camSetup.Rotation * Vector3.up * offset.y;

			camSetup.Position = Local.Pawn.EyePos + relativeOffset + camSetup.Rotation * Vector3.back * ((ray ? hitInfo.distance : distance) - padding);
		}

		protected override void OnBuildControls( ref IControls.Setup setup )
		{
			base.OnBuildControls( ref setup );
			setup.ViewAngles.x = Mathf.Clamp( setup.ViewAngles.x, -60, 88 );
		}

		// Fields

		public Vector2 offset;

		[SerializeField]
		private float smoothing = 20;

		[SerializeField]
		private float distance = 5;

		[SerializeField]
		private float padding = 0.1f;
	}
}
