using Sandbox;
using Sandbox.UI;
using System.ComponentModel;

namespace MurderGame;

public partial class Player : AnimatedEntity
{
	[ClientInput]
	public Vector3 InputDirection { get; set; }
	
	[ClientInput]
	public Angles ViewAngles { get; set; }

	[Browsable( false )]
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	[Net, Predicted, Browsable( false )]
	public Vector3 EyeLocalPosition { get; set; }

	[Browsable( false )]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	[Net, Predicted, Browsable( false )]
	public Rotation EyeLocalRotation { get; set; }

	public BBox Hull
	{
		get => new
		(
			new Vector3( -12, -12, 0 ),
			new Vector3( 12, 12, 64 )
		);
	}

	[Net]
	public Team CurrentTeam { get; set; }

	[BindComponent] public PlayerController Controller { get; }
	[BindComponent] public PlayerAnimator Animator { get; }
	[BindComponent] public PlayerInventory Inventory { get; }
	[BindComponent] public PlayerSpectator Spectator { get; }

	public Ragdoll PlayerRagdoll { get; set; }
	public ClothingContainer PlayerClothingContainer { get; set; }

	public Vector3 LastAttackForce { get; set; }
	public int LastHitBone { get; set; }

	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Tags.Add( "player" );
		EnableHitboxes = true;
		EnableDrawing = false;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Hull.Mins, Hull.Maxs );
		EnableSolidCollisions = false;
	}

	public void Respawn()
	{
		Tags.Add( "livingplayer" );
		EnableAllCollisions = true;
		EnableDrawing = true;
		Components.Remove( Spectator );
		Components.Create<PlayerController>();
		Components.Create<PlayerAnimator>();
		Components.Create<PlayerInventory>();
		Health = 100f;
		DeleteRagdoll();
	}

	public void Cleanup()
	{
		DisablePlayer();
		DeleteRagdoll();
		Components.RemoveAll();
	}

	public void DeleteRagdoll()
	{
		if (PlayerRagdoll != null)
		{
			PlayerRagdoll.Delete();
			PlayerRagdoll = null;
		}
	}

	public void DisablePlayer()
	{
		EnableAllCollisions = false;
		LifeState = LifeState.Dead;
		Tags.Remove( "livingplayer" );
		Inventory?.Clear();
		Components.RemoveAll();
		EnableDrawing = false;
	}

	public override void OnKilled()
	{
		Inventory?.SpillContents(EyePosition, new Vector3(0,0,0));
		DisablePlayer();
		Event.Run( MurderEvent.Kill, LastAttacker, this );
		var ragdoll = new Ragdoll();
		ragdoll.Position = Position;
		ragdoll.Rotation = Rotation;
		ragdoll.CopyFrom(this);
		ragdoll.PhysicsGroup.AddVelocity(LastAttackForce / 100);
		PlayerClothingContainer.DressEntity( ragdoll );
		PlayerRagdoll = ragdoll;
		Components.Create<PlayerSpectator>();
	}

	public override void TakeDamage( DamageInfo info )
	{
		LastAttacker = info.Attacker;
		LastAttackerWeapon = info.Weapon;
		LastAttackForce = info.Force;
		LastHitBone = info.BoneIndex;
		if (Game.IsServer && Health > 0f && LifeState == LifeState.Alive)
		{
			Health -= info.Damage;
			if (Health <= 0f)
			{
				Health = 0f;
				OnKilled();
			}
		}
	}

	public void DressFromClient( IClient cl )
	{
		PlayerClothingContainer = new ClothingContainer();
		PlayerClothingContainer.LoadFromClient( cl );
		PlayerClothingContainer.DressEntity( this );
	}

	public override void Simulate( IClient cl )
	{
		SimulateRotation();
		Controller?.Simulate( cl );
		Animator?.Simulate();
		Inventory?.Simulate( cl );
		Spectator?.Simulate();
		EyeLocalPosition = Vector3.Up * (64f * Scale);
	}

	public override void BuildInput()
	{
		InputDirection = Input.AnalogMove;

		if ( Input.StopProcessing )
			return;

		var look = Input.AnalogLook;

		if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
		{
			look = look.WithYaw( look.yaw * -1f );
		}

		var viewAngles = ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
		viewAngles.roll = 0f;
		ViewAngles = viewAngles.Normal;
	}

	public override void FrameSimulate( IClient cl )
	{
		if (Spectator != null)
		{
			Spectator.FrameSimulate(this);
			return;
		}
		SimulateRotation();

		Camera.Rotation = ViewAngles.ToRotation();
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		Camera.FirstPersonViewer = this;
		Camera.Position = EyePosition;
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, Hull.Mins, Hull.Maxs, liftFeet );
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "player", "passbullets" )
					.Ignore( this )
					.Run();

		return tr;
	}

	protected void SimulateRotation()
	{
		EyeRotation = ViewAngles.ToRotation();
		Rotation = ViewAngles.WithPitch( 0f ).ToRotation();
	}

}
