using MurderGame;
using Sandbox;
using System;
using Sandbox.Component;

public partial class DroppedWeapon : AnimatedEntity, IUse
{
	private int Ammo { get; }
	private Type WeaponType { get; }

	public DroppedWeapon(Weapon weapon) : this()
	{
		Ammo = weapon.Ammo;
		WeaponType = weapon.GetType();
	}

	public DroppedWeapon()
	{
		Tags.Add("droppedweapon");
		
		var glow = Components.GetOrCreate<Glow>();
		glow.Enabled = true;
		glow.Width = 0.25f;
		glow.Color = new Color( 4f, 50.0f, 70.0f, 1.0f );
		glow.ObscuredColor = new Color( 0, 0, 0, 0);
		
		PhysicsEnabled = true;
		UsePhysicsCollision = true;
		EnableSelfCollisions = true;
		EnableSolidCollisions = true;
	}

	public override void StartTouch( Entity other )
	{
		if ( !Game.IsServer ) return;
		if ( !other.Tags.Has( "livingplayer" ) ) return;

		var player = (MurderGame.Player)other;
		if ( IsUsable( player ) )
		{
			Pickup( player );
		}
	}

	public void Pickup( MurderGame.Player player )
	{
		var instance = TypeLibrary.Create<Weapon>( WeaponType );
		instance.Ammo = Ammo;
		player.Inventory.SetPrimaryWeapon( instance );
		Delete();
	}

	public bool OnUse( Entity user )
	{
		if ( user is not MurderGame.Player player ) return false;
		Pickup( player );
		return false;
	}

	public bool IsUsable( Entity user )
	{
		return user is MurderGame.Player { Inventory: { PrimaryWeapon: null, AllowPickup: true } };
	}
}
