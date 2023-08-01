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
			new Vector3( -16, -16, 0 ),
			new Vector3( 16, 16, 72 )
		);
	}


	public BaseCameraComponent Camera => Components.Get<BaseCameraComponent>();
	public BaseControllerComponent Controller => Components.Get<BaseControllerComponent>();
	[BindComponent] public AnimatorComponent Animator { get; }
	[BindComponent] public InventoryComponent Inventory { get; }
	[BindComponent] public FallDamageComponent FallDamage { get; }
	[BindComponent] public FootprintTrackerComponent FootprintTracker { get; }

	[Net]
	public Ragdoll PlayerRagdoll { get; set; }
	public ClothingContainer PlayerClothingContainer { get; set; }

	public Vector3 LastAttackForce { get; set; }
	public int LastHitBone { get; set; }

	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	[Net, Predicted]
	public TimeSince TimeSinceDeath { get; set; } = 0;
	
	public Player LookingAt { get; set; }
	
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

		Health = 0f;
		LifeState = LifeState.Dead;
	}

	public void Respawn()
	{
		DeleteRagdoll();
		Tags.Add( "livingplayer" );

		EnableAllCollisions = true;
		EnableDrawing = true;

		Components.Create<WalkControllerComponent>();
		Components.Create<PlayerCameraComponent>();
		Components.Create<AnimatorComponent>();
		Components.Create<InventoryComponent>();
		Components.Create<FallDamageComponent>();

		Health = 100f;
		LifeState = LifeState.Alive;
	}

	public void Cleanup()
	{
		DisablePlayer();
		DeleteRagdoll();
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
		Tags.Remove( "livingplayer" );

		EnableAllCollisions = false;
		EnableDrawing = false;

		Inventory?.Clear();

		LifeState = LifeState.Dead;
	}

	public override void OnKilled()
	{
		TimeSinceDeath = 0;

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

		DeathOverlay.Show( To.Single( Client ) );
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
	
	public void Dress()
	{
		PlayerClothingContainer = new ClothingContainer();
		var trousers = new Clothing() { Model = "models/citizen_clothes/trousers/cargopants/models/cargo_pants.vmdl" };
		var tshirt = new Clothing() { Model = "models/citizen_clothes/shirt/Tshirt/Models/tshirt.vmdl" };
		PlayerClothingContainer.Clothing.Add( trousers );
		PlayerClothingContainer.Clothing.Add( tshirt );
		PlayerClothingContainer.DressEntity( this );
	}

	public override void Simulate( IClient cl )
	{
		SimulateRotation();
		TickPlayerUse();

		Controller?.Simulate( cl );
		Camera?.Simulate( cl );
		Animator?.Simulate();
		Inventory?.Simulate( cl );
		FallDamage?.Simulate( cl );
		FootprintTracker?.Simulate( cl );

		if (Game.IsServer && Camera is not SpectatorCameraComponent && LifeState == LifeState.Dead && TimeSinceDeath > 3.5)
		{
			DeathOverlay.Hide( To.Single( Client ) );
			Components.Remove( Controller );
			Components.Remove( Camera );
			Components.Remove( Animator );
			Components.Remove( Inventory );
			Components.Remove( FallDamage );
			Components.Create<SpectatorCameraComponent>();
		}

		if ( Game.IsClient )
		{
			var start = AimRay.Position;
			var end = AimRay.Position + AimRay.Forward * 5000;
			
			var trace = Trace.Ray( start, end )
					.UseHitboxes()
					.WithAnyTags( "solid", "livingplayer" )
					.Ignore( this )
					.Size( 1f );
			
			var tr = trace.Run();
			if ( tr.Hit && tr.Entity.IsValid() && tr.Entity is Player player )
			{
				LookingAt = player;
			}
			else
			{
				LookingAt = null;
			}
		}
			

	}

	public override void BuildInput()
	{
		Controller?.BuildInput();
		Camera?.BuildInput();
	}

	public override void FrameSimulate( IClient cl )
	{
		Controller?.FrameSimulate( cl );
		Camera?.FrameSimulate( cl );
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
