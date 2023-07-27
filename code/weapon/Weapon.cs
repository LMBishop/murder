using Sandbox;
using System.Collections.Generic;

namespace MurderGame;

public partial class Weapon : AnimatedEntity
{
	public WeaponViewModel ViewModelEntity { get; protected set; }
	public BaseViewModel HandModelEntity { get; protected set; }

	public Player Pawn => Owner as Player;

	public AnimatedEntity EffectEntity => Camera.FirstPersonViewer == Owner ? ViewModelEntity : this;

	public virtual string ViewModelPath => null;
	public virtual string ModelPath => null;
	public virtual string HandsModelPath => null;

	public virtual float PrimaryRate => 5f;
	public virtual float ReloadTime => 3.5f;

	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }

	[Net, Predicted] public TimeSince TimeSinceReload { get; set; }
	[Net, Predicted] public TimeUntil TimeUntilReloadComplete { get; set; }
	[Net, Predicted] public bool Reloading { get; set; }

	[Net, Predicted] public int Ammo { get; set; }
	[Net, Predicted] public int MaxAmmo { get; set; }

	public override void Spawn()
	{
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableDrawing = false;

		if ( ModelPath != null )
		{
			SetModel( ModelPath );
		}
	}

	public void ChangeOwner( Player pawn )
	{
		Owner = pawn;
		SetParent( pawn, true );
	}

	public void OnEquip( Player pawn )
	{
		if (Owner == null)
		{
			Owner = pawn;
			SetParent( pawn, true );
		}
		EnableDrawing = true;
		CreateViewModel( To.Single( pawn ) );
	}

	public void OnHolster()
	{
		Reloading = false;
		EnableDrawing = false;
		DestroyViewModel( To.Single( Owner ) );
		Owner = null;
	}

	public override void Simulate( IClient player )
	{
		Animate();
		if (Reloading && TimeUntilReloadComplete)
		{
			Reloading = false;
			Ammo = MaxAmmo;
		}

		if ( CanPrimaryAttack() )
		{
			using ( LagCompensation() )
			{
				TimeSincePrimaryAttack = 0;
				PrimaryAttack();
			}
		}
		else if (Input.Down("reload") && !Reloading && Ammo != MaxAmmo)
		{
			Reload();
			Reloading = true;
			TimeUntilReloadComplete = ReloadTime;
		}
	}

	public virtual bool CanPrimaryAttack()
	{
		if ( !Owner.IsValid() || !Input.Down( "attack1" ) ) return false;

		var rate = PrimaryRate;
		if ( rate <= 0 ) return true;

		return !Reloading && TimeSincePrimaryAttack > (1 / rate);
	}

	public virtual void PrimaryAttack()
	{
	}

	public virtual void Reload()
	{
	}

	protected virtual void Animate()
	{
	}

	public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		bool underWater = Trace.TestPoint( start, "water" );

		var trace = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "livingplayer", "npc" )
				.Ignore( this )
				.Size( radius );

		if ( !underWater )
			trace = trace.WithAnyTags( "water" );

		var tr = trace.Run();

		if ( tr.Hit )
			yield return tr;
	}

	public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
	{
		var forward = dir;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;

		foreach ( var tr in TraceBullet( pos, pos + forward * 5000, bulletSize ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !Game.IsServer ) continue;
			if ( !tr.Entity.IsValid() || !tr.Entity.Tags.Has("player") ) continue;

			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}

	public virtual void ShootBullet( float force, float damage, float bulletSize )
	{
		Game.SetRandomSeed( Time.Tick );

		var ray = Owner.AimRay;
		ShootBullet( ray.Position, ray.Forward, 0, force, damage, bulletSize );
	}
	
	public virtual bool Melee( float force, float damage )
	{
		var ray = Owner.AimRay;
		var forward = ray.Forward.Normal;
		var pos = ray.Position;
		bool hit = false;

		foreach (var tr in TraceBullet(pos, pos + forward * 50, 20))
		{
			tr.Surface.DoBulletImpact(tr);
			hit = true;

			if ( !Game.IsServer ) continue;
			if ( !tr.Entity.IsValid() || !tr.Entity.Tags.Has("player") ) continue;

			using (Prediction.Off())
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward.Normal * 100 * force, damage )
					.UsingTraceResult(tr)
					.WithAttacker(Owner)
					.WithWeapon(this);

				tr.Entity.TakeDamage(damageInfo);
			}
		}
		return hit;
	}

	[ClientRpc]
	public void CreateViewModel()
	{
		if ( ViewModelPath == null ) return;

		var vm = new WeaponViewModel( this );
		vm.Model = Model.Load( ViewModelPath );
		vm.Owner = Owner;
		ViewModelEntity = vm;
		if (!string.IsNullOrEmpty(HandsModelPath))
		{
			HandModelEntity = new BaseViewModel();
			HandModelEntity.Owner = Owner;
			HandModelEntity.EnableViewmodelRendering = true;
			HandModelEntity.SetModel(HandsModelPath);
			HandModelEntity.SetParent(ViewModelEntity, true);
		}
	}

	[ClientRpc]
	public void DestroyViewModel()
	{
		if ( ViewModelEntity.IsValid() )
		{
			ViewModelEntity.Delete();
		}
		if ( HandModelEntity.IsValid() )
		{
			HandModelEntity.Delete();
		}
	}
}
