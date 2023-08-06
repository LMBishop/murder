using Sandbox;

namespace MurderGame;

public class WeaponViewModel : BaseViewModel
{
	public WeaponViewModel( Weapon weapon )
	{
		Weapon = weapon;
		EnableShadowCasting = false;
		EnableViewmodelRendering = true;
	}

	protected Weapon Weapon { get; init; }

	public override void PlaceViewmodel()
	{
		base.PlaceViewmodel();

		Camera.Main.SetViewModelCamera( 80f );
	}
}
