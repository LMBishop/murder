using System;
using MurderGame;
using Sandbox;
using Sandbox.Component;
using Player = MurderGame.Player;

public class DroppedWeapon : AnimatedEntity, IUse
{
	public DroppedWeapon( Weapon weapon ) : this()
	{
		Ammo = weapon.Ammo;
		WeaponType = weapon.GetType();
	}

	public DroppedWeapon()
	{
		Tags.Add( "droppedweapon" );

		var glow = Components.GetOrCreate<Glow>();
		glow.Enabled = true;
		glow.Width = 0.25f;
		glow.Color = new Color( 4f, 50.0f, 70.0f );
		glow.ObscuredColor = new Color( 0, 0, 0, 0 );

		PhysicsEnabled = true;
		UsePhysicsCollision = true;
		EnableSelfCollisions = true;
		EnableSolidCollisions = true;
	}

	private int Ammo { get; }
	private Type WeaponType { get; }

	public bool OnUse( Entity user )
	{
		if ( user is not Player player )
		{
			return false;
		}

		Pickup( player );
		return false;
	}

	public bool IsUsable( Entity user )
	{
		return user is Player { Inventory: { PrimaryWeapon: null, AllowPickup: true } };
	}

	public override void StartTouch( Entity other )
	{
		if ( !Game.IsServer )
		{
			return;
		}

		if ( !other.Tags.Has( "livingplayer" ) )
		{
			return;
		}

		var player = (Player)other;
		if ( IsUsable( player ) )
		{
			Pickup( player );
		}
	}

	public void Pickup( Player player )
	{
		var instance = TypeLibrary.Create<Weapon>( WeaponType );
		instance.Ammo = Ammo;
		player.Inventory.SetPrimaryWeapon( instance );
		Delete();
	}
}
