using Sandbox;
using System;

namespace MurderGame;

public partial class InventoryComponent : EntityComponent<Player>, ISingletonComponent
{
	const int MIN_SLOT = 1;
	const int MAX_SLOT = 2;

	const int UNARMED_SLOT = 1;
	const int PRIMARY_SLOT = 2;

	[Net]
	public Weapon PrimaryWeapon { get; private set; }

	[Net]
	public int ActiveSlot { get; set; }

	[Net]
	public bool AllowPickup { get; set; } = true;

	public Weapon GetCurrentWeapon()
	{
		return ActiveSlot switch
		{
			PRIMARY_SLOT => PrimaryWeapon,
			_ => null,
		};
	}

	public void SetPrimaryWeapon( Weapon weapon )
	{
		PrimaryWeapon?.OnHolster();
		PrimaryWeapon?.Delete();
		PrimaryWeapon = weapon;
		if (weapon != null)
		{
			weapon.ChangeOwner( Entity );
		}
		if (ActiveSlot == PRIMARY_SLOT)
		{
			weapon?.OnEquip( Entity );
		}
	}

	private void PrevSlot()
	{
		if (ActiveSlot > MIN_SLOT)
		{
			--ActiveSlot;
		}
		else
		{
			ActiveSlot = MAX_SLOT;
		}
	}
	private void NextSlot()
	{
		if (ActiveSlot < MAX_SLOT)
		{
			++ActiveSlot;
		}
		else
		{
			ActiveSlot = MIN_SLOT;
		}
	}

	public void Simulate(IClient cl)
	{
		var currentWeapon = GetCurrentWeapon();
		var currentSlot = ActiveSlot;

		if (Input.Released("SlotPrev"))
		{
			PrevSlot();
		}
		else if (Input.Released("SlotNext"))
		{
			NextSlot();
		}
		else if (Input.Down("Slot1"))
		{
			ActiveSlot = 1;
		}
		else if (Input.Down("Slot2"))
		{
			ActiveSlot = 2;
		}

		if (ActiveSlot != currentSlot)
		{
			currentWeapon?.OnHolster();
			GetCurrentWeapon()?.OnEquip( Entity );
		}
		GetCurrentWeapon()?.Simulate( cl );
	}

	public void Holster()
	{
		Weapon weapon = GetCurrentWeapon();
		weapon?.OnHolster();
	}

	public void Clear()
	{
		Holster();
		SetPrimaryWeapon( null );
	}

	public void SpillContents(Vector3 location, Vector3 velocity)
	{
		Holster();
		if (PrimaryWeapon is not null and Pistol )
		{
			PrimaryWeapon.ChangeOwner( null );
			DroppedWeapon droppedWeapon = new( (Pistol)PrimaryWeapon );
			droppedWeapon.CopyFrom( PrimaryWeapon );
			droppedWeapon.Position = location;
			droppedWeapon.Velocity = velocity;
		}
		Clear();
	}

}
