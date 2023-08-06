using Sandbox;

namespace MurderGame;

public partial class Knife : Weapon
{
	public Knife()
	{
		Ammo = -1;
		MaxAmmo = -1;
	}

	public override string ModelPath => "weapons/swb/melee/bayonet/w_bayonet.vmdl";
	public override string ViewModelPath => "weapons/swb/melee/bayonet/v_bayonet.vmdl";
	public override string HandsModelPath => "weapons/swb/hands/rebel/v_hands_rebel.vmdl";
	public override float PrimaryRate => 1.5f;

	[ClientRpc]
	protected virtual void ShootEffects( bool hit )
	{
		Game.AssertClient();

		ViewModelEntity?.SetAnimParameter( hit ? "swing" : "swing_miss", true );
	}

	public override void PrimaryAttack()
	{
		Pawn?.SetAnimParameter( "b_attack", true );
		Pawn?.PlaySound( "bayonet.slash" );
		ShootEffects( Melee( 100, 100 ) );
	}

	protected override void Animate()
	{
		Pawn?.SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.Swing );
	}
}
