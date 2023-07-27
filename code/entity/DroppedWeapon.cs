using MurderGame;
using Sandbox;
using System;

public partial class DroppedWeapon : AnimatedEntity 
{
	public int Ammo { get; set; }
	public Type WeaponType { get; set; }

	public DroppedWeapon(Weapon weapon) : this()
	{
		Ammo = weapon.Ammo;
		WeaponType = weapon.GetType();
	}

	public DroppedWeapon()
	{
		Tags.Add("droppedweapon");
		PhysicsEnabled = true;
		UsePhysicsCollision = true;
		EnableSelfCollisions = true;
		EnableSolidCollisions = true;
	}

	public override void StartTouch( Entity other )
	{
		if ( !Game.IsServer ) return;
		if ( !other.Tags.Has( "livingplayer" ) ) return;

        MurderGame.Player player = (MurderGame.Player)other;

		if ( player.Inventory == null || player.Inventory.PrimaryWeapon != null || !player.Inventory.AllowPickup) return;

		Weapon instance = TypeLibrary.Create<Weapon>( WeaponType );
		instance.Ammo = Ammo;
		player.Inventory.SetPrimaryWeapon( instance );
		Delete();
	}
}
