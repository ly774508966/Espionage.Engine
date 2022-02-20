﻿using UnityEngine;

namespace Espionage.Engine
{
	public class FirstPersonController : PawnController
	{
		protected override void OnAwake()
		{
			Controller = GetComponent<CharacterController>();
		}

		public CharacterController Controller { get; private set; }

		public override void Simulate( Client client )
		{
			base.Simulate( client );

			var input = client.Input;
			var rot = transform.rotation;

			// Start Gravity
			Velocity -= new Vector3( 0, gravity / 2, 0 ) * Time.deltaTime;

			if ( Controller.isGrounded )
			{
				var cappedVel = Velocity;
				cappedVel.y = 0;
				Velocity = cappedVel;

				ApplyFriction( friction, stopSpeed );
			}

			// Smooth WishSpeed, so it isn't jarring
			WishSpeed = Mathf.Lerp( WishSpeed, GrabWishSpeed( client ), 6 * Time.deltaTime );

			var wishDir = rot * Vector3.forward * input.Forward + rot * Vector3.right * input.Horizontal;
			wishDir = wishDir.normalized * WishSpeed * Time.deltaTime;

			Accelerate( wishDir.normalized, wishDir.magnitude, 0, 10 );

			// Finish Gravity
			Velocity -= new Vector3( 0, gravity / 2, 0 ) * Time.deltaTime;

			if ( Controller.isGrounded )
			{
				var cappedVel = Velocity;
				cappedVel.y = 0;
				Velocity = cappedVel;
			}

			Controller.Move( Velocity );
		}

		// Wish Speed

		public float WishSpeed { get; protected set; }

		protected virtual float GrabWishSpeed( Client cl )
		{
			if ( Input.GetKey( KeyCode.LeftShift ) && Mathf.Abs( Controller.velocity.magnitude ) > 0 )
			{
				return sprintSpeed;
			}

			return walkSpeed;
		}

		// Helpers

		protected virtual void Accelerate( Vector3 wishDir, float wishSpeed, float speedLimit, float acceleration )
		{
			if ( speedLimit > 0 && wishSpeed > speedLimit )
			{
				wishSpeed = speedLimit;
			}

			// See if we are changing direction a bit
			var currentSpeed = Vector3.Dot( Velocity, wishDir );

			// Reduce wishSpeed by the amount of veer.
			var addSpeed = wishSpeed - currentSpeed;

			// If not going to add any speed, done.
			if ( addSpeed <= 0 )
			{
				return;
			}

			// Determine amount of acceleration.
			var accelSpeed = acceleration * Time.deltaTime * wishSpeed;

			// Cap at addSpeed
			if ( accelSpeed > addSpeed )
			{
				accelSpeed = addSpeed;
			}

			Velocity += wishDir * accelSpeed;
		}

		protected virtual void ApplyFriction( float frictionAmount = 10.0f, float stopPower = 100f )
		{
			// Calculate speed
			var speed = Velocity.magnitude;
			if ( speed < 0.1f )
			{
				return;
			}

			// Bleed off some speed, but if we have less than the bleed
			//  threshold, bleed the threshold amount.
			var control = speed < stopPower ? stopPower : speed;

			// Add the amount to the drop amount.
			var drop = control * Time.deltaTime * frictionAmount;

			// scale the velocity
			var newSpeed = speed - drop;
			if ( newSpeed < 0 )
			{
				newSpeed = 0;
			}

			if ( newSpeed != speed )
			{
				newSpeed /= speed;
				Velocity *= newSpeed;
			}
		}

		// Move

		protected virtual void GroundedMove() { }

		// Fields

		[SerializeField]
		private float walkSpeed = 15;

		[SerializeField]
		private float sprintSpeed = 15;

		[SerializeField]
		private float gravity = 20;

		[SerializeField]
		private float friction = 10;

		[SerializeField]
		private float stopSpeed = 100;
	}
}
