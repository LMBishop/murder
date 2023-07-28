using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace MurderGame;

public class PlayerSpectator : EntityComponent<Player>
{
	public Player Target { get; set; }

	public void Simulate()
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

	public void FrameSimulate( Player player )
	{
		if ( Target == null || !Target.IsValid() || Target.LifeState == LifeState.Dead ) return;

		// SimulateRotation(player);
		Camera.Rotation = Target.EyeRotation;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		Camera.FirstPersonViewer = Target;
		Camera.Position = Target.EyePosition;
	}

	protected void SimulateRotation(Player player)
	{
		player.EyeRotation = Target.ViewAngles.ToRotation();
		player.Rotation = Target.ViewAngles.WithPitch( 0f ).ToRotation();
	}

	public List<IClient> GetTargets()
	{
		return Game.Clients.Where(c => c.Pawn is Player player && player.CurrentTeam != Team.Spectator && player.LifeState == LifeState.Alive).ToList();
	}
}
