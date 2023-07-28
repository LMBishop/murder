using Sandbox;
using System;
using System.Collections.Generic;

namespace MurderGame;

public class PlayerController : EntityComponent<Player>
{
	public int StepSize => 24;
	public int GroundAngle => 45;
	public int JumpSpeed => 300;
	public float Gravity => 800f;
	public float SpeedMultiplier { get; set; } = 1;

	HashSet<string> ControllerEvents = new( StringComparer.OrdinalIgnoreCase );

	bool IsTouchingLadder = false;

	Vector3 LadderNormal;

	bool Grounded => Entity.GroundEntity.IsValid();

	public void Simulate( Player player )
	{
		ControllerEvents.Clear();

		var movement = Entity.InputDirection.Normal;
		var angles = Entity.ViewAngles.WithPitch( 0 );
		var moveVector = Rotation.From( angles ) * movement * 320f;
		var groundEntity = CheckForGround();
		var team = Entity.CurrentTeam;

		// wasd -- start

		if ( groundEntity.IsValid() )
		{
			if ( !Grounded )
			{
				Entity.Velocity = Entity.Velocity.WithZ( 0 );
				AddEvent( "grounded" );
			}
			var sprintMultiplier = TeamOperations.CanSprint( team ) ? (Input.Down( "run" ) ? 2.5f : 1f) : 1f;

			Entity.Velocity = Accelerate( Entity.Velocity, moveVector.Normal, moveVector.Length, SpeedMultiplier * 200.0f * sprintMultiplier, 7.5f );
			Entity.Velocity = ApplyFriction( Entity.Velocity, 4.0f );
		}
		else
		{
			Entity.Velocity = Accelerate( Entity.Velocity, moveVector.Normal, moveVector.Length, SpeedMultiplier * 100, 20f );
			Entity.Velocity += Vector3.Down * Gravity * Time.Delta * (1/SpeedMultiplier);
		}

		// wasd -- end

		// ladder -- start

		if (CheckLadder())
		{
			float normalDot = moveVector.Dot(LadderNormal);
			var cross = LadderNormal * normalDot;
			Entity.Velocity = (moveVector - cross) + (-normalDot * LadderNormal.Cross(Vector3.Up.Cross(LadderNormal).Normal) * 0.5f);
		}

		// ladder -- end

		if ( Input.Pressed( "jump" ) )
		{
			DoJump();
		}

		// actually do move --

		var mh = new MoveHelper( Entity.Position, Entity.Velocity );
		mh.Trace = mh.Trace.Size( Entity.Hull ).Ignore( Entity );

		if (mh.TryUnstuck())
		{
			Entity.Position = mh.Position;
			Entity.Velocity = mh.Velocity;
		}

		if ( mh.TryMoveWithStep( Time.Delta, StepSize ) > 0 )
		{
			if ( Grounded )
			{
				mh.Position = StayOnGround( mh.Position );
			}
			Entity.Position = mh.Position;
			Entity.Velocity = mh.Velocity;
		}
		Entity.GroundEntity = groundEntity;
	}

	void DoJump()
	{
		if ( Grounded )
		{
			Entity.Velocity = ApplyJump( Entity.Velocity, "jump" );
		}
	}

	public bool CheckLadder()
	{
		var wishvel = new Vector3( Entity.InputDirection.x.Clamp( -1f, 1f ), Entity.InputDirection.y.Clamp( -1f, 1f ), 0);
		wishvel *= Entity.ViewAngles.WithPitch(0).ToRotation();
		wishvel = wishvel.Normal;

		if (IsTouchingLadder)
		{
			if (Input.Pressed("jump"))
			{
				Entity.Velocity = LadderNormal * 100.0f;
				return false;

			}
			else if (Entity.GroundEntity != null && LadderNormal.Dot(wishvel) > 0)
			{
				return false;
			}
		}

		const float ladderDistance = 1.0f;
		var start = Entity.Position;
		Vector3 end = start + (IsTouchingLadder ? (LadderNormal * -1.0f) : wishvel) * ladderDistance;

		var pm = Trace.Ray(start, end)
					.Size(Entity.Hull.Mins, Entity.Hull.Maxs)
					.WithTag("ladder")
					.Ignore(Entity)
					.Run();

		if (pm.Hit)
		{
			LadderNormal = pm.Normal;
			return true;
		}
		return false;
	}

	Entity CheckForGround()
	{
		if ( Entity.Velocity.z > 100f )
			return null;

		var trace = Entity.TraceBBox( Entity.Position, Entity.Position + Vector3.Down, 2f );

		if ( !trace.Hit )
			return null;

		if ( trace.Normal.Angle( Vector3.Up ) > GroundAngle )
			return null;

		return trace.Entity;
	}

	Vector3 ApplyFriction( Vector3 input, float frictionAmount )
	{
		float StopSpeed = 100.0f;

		var speed = input.Length;
		if ( speed < 0.1f ) return input;

		// Bleed off some speed, but if we have less than the bleed
		// threshold, bleed the threshold amount.
		float control = (speed < StopSpeed) ? StopSpeed : speed;

		// Add the amount to the drop amount.
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity
		float newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;
		if ( newspeed == speed ) return input;

		newspeed /= speed;
		input *= newspeed;

		return input;
	}

	Vector3 Accelerate( Vector3 input, Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && wishspeed > speedLimit )
			wishspeed = speedLimit;

		var currentspeed = input.Dot( wishdir );
		var addspeed = wishspeed - currentspeed;

		if ( addspeed <= 0 )
			return input;

		var accelspeed = acceleration * Time.Delta * wishspeed;

		if ( accelspeed > addspeed )
			accelspeed = addspeed;

		input += wishdir * accelspeed;

		return input;
	}

	Vector3 ApplyJump( Vector3 input, string jumpType )
	{
		AddEvent( jumpType );

		return input + Vector3.Up * JumpSpeed;
	}

	Vector3 StayOnGround( Vector3 position )
	{
		var start = position + Vector3.Up * 2;
		var end = position + Vector3.Down * StepSize;

		// See how far up we can go without getting stuck
		var trace = Entity.TraceBBox( position, start );
		start = trace.EndPosition;

		// Now trace down from a known safe position
		trace = Entity.TraceBBox( start, end );

		if ( trace.Fraction <= 0 ) return position;
		if ( trace.Fraction >= 1 ) return position;
		if ( trace.StartedSolid ) return position;
		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return position;

		return trace.EndPosition;
	}

	public bool HasEvent( string eventName )
	{
		return ControllerEvents.Contains( eventName );
	}

	void AddEvent( string eventName )
	{
		if ( HasEvent( eventName ) )
			return;

		ControllerEvents.Add( eventName );
	}
}
