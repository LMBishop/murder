using Sandbox;

namespace MurderGame;

public partial class Pistol : Weapon
{
	public Pistol()
	{
		MaxAmmo = 1;
	}

	public override string ModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Game.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		Pawn?.SetAnimParameter( "b_attack", true );
		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	public override void PrimaryAttack()
	{
		if ( Ammo > 0 )
		{
			--Ammo;
			ShootEffects();
			Pawn?.PlaySound( "rust_pistol.shoot" );
			ShootBullet( 100, 100, 1 );
		}
	}

	public override void Reload()
	{
		ReloadEffects();
		Pawn?.SetAnimParameter( "b_reload", true );
	}

	[ClientRpc]
	protected virtual void ReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );
	}

	protected override void Animate()
	{
		Pawn?.SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.Pistol );
	}
}
