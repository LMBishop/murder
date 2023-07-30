using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace MurderGame;

public class SpectatorCameraComponent : BaseCameraComponent
{
	public Player Target { get; set; }

	public override void Simulate( IClient cl )
	{
		if (Target == null || !Target.IsValid() || Target.LifeState == LifeState.Dead)
		{
			var targets = GetTargets();
			if ( targets.Count == 0 )
			{
				Target = null;
				return;
			}
			var nextTarget = targets.First();
			Target = (Player)nextTarget.Pawn;
		}
	}

	public override void FrameSimulate( IClient cl )
	{
		if ( Target == null || !Target.IsValid() || Target.LifeState == LifeState.Dead ) return;

		Camera.Rotation = Target.EyeRotation;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		Camera.FirstPersonViewer = Target;
		Camera.Position = Target.EyePosition;
	}

	private List<IClient> GetTargets()
	{
		return Game.Clients.Where(c => c.Pawn is Player player && player.CurrentTeam != Team.Spectator && player.LifeState == LifeState.Alive).ToList();
	}

	public override InventoryComponent GetObservedInventory()
	{
		return Target?.Inventory;
	}
	
	public override float GetObservedHealth()
	{
		return Target?.Health ?? Entity.Health;
	}

	public override Team GetObservedTeam()
	{
		return Target?.CurrentTeam ?? Entity.CurrentTeam;
	}
}
