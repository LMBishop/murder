using Sandbox;

namespace MurderGame;

public class PlayerCameraComponent : BaseCameraComponent
{
	protected override void OnActivate()
	{
		base.OnActivate();
		// Set field of view to whatever the user chose in options
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
	}
	public override void FrameSimulate( IClient cl )
	{

		var pl = Entity as Player;
		// Update rotation every frame, to keep things smooth  

		if (pl.PlayerRagdoll != null && pl.LifeState == LifeState.Dead)
		{
			Camera.Position = pl.PlayerRagdoll.Position;
			Camera.FirstPersonViewer = pl.PlayerRagdoll;
			return;
		}

		pl.EyeRotation = pl.ViewAngles.ToRotation();

		Camera.Position = pl.EyePosition;
		Camera.Rotation = pl.ViewAngles.ToRotation();

		Camera.Main.SetViewModelCamera( Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView ) );

		// Set the first person viewer to this, so it won't render our model
		Camera.FirstPersonViewer = Entity;

		Camera.ZNear = 8 * pl.Scale;
	}
	public override void BuildInput()
	{
		if ( Game.LocalClient.Components.TryGet<DevCamera>( out var _ ) )
			return;

		var pl = Entity as Player;
		var viewAngles = (pl.ViewAngles + Input.AnalogLook).Normal;
		pl.ViewAngles = viewAngles.WithPitch( viewAngles.pitch.Clamp( -89f, 89f ) );
		return;
	}
	
	public override InventoryComponent GetObservedInventory()
	{
		return Entity.Inventory;
	}

	public override float GetObservedHealth()
	{
		return Entity.Health;
	}

	public override Team GetObservedTeam()
	{
		return Entity.CurrentTeam;
	}
}
