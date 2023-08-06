using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace MurderGame;

public partial class SpectatorCameraComponent : BaseCameraComponent
{
	[Net] public Player Target { get; set; }
	[Net] public int TargetIndex { get; set; }

	public override void Simulate( IClient cl )
	{
		var targets = GetTargets();
		if ( targets.Count == 0 )
		{
			Target = null;
			return;
		}

		if ( Target == null || !Target.IsValid() || Target.LifeState == LifeState.Dead )
		{
			FindNextTarget( targets, false );
			return;
		}

		if ( Input.Released( "attack1" ) )
		{
			FindNextTarget( targets, false );
		}
		else if ( Input.Released( "attack2" ) )
		{
			FindNextTarget( targets, true );
		}
	}


	public override void FrameSimulate( IClient cl )
	{
		if ( Target == null || !Target.IsValid() || Target.LifeState == LifeState.Dead )
		{
			return;
		}

		Camera.Rotation = Target.EyeRotation;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		Camera.FirstPersonViewer = Target;
		Camera.Position = Target.EyePosition;
	}

	private List<IClient> GetTargets()
	{
		return Game.Clients.Where( c =>
			c.Pawn is Player player && player.Team != Team.Spectator && player.LifeState == LifeState.Alive ).ToList();
	}

	private void FindNextTarget( List<IClient> targets, bool backwards )
	{
		if ( !backwards )
		{
			if ( ++TargetIndex >= targets.Count )
			{
				TargetIndex = 0;
			}
		}
		else
		{
			if ( --TargetIndex < 0 )
			{
				TargetIndex = targets.Count - 1;
			}
		}

		var nextTarget = targets[TargetIndex];
		Target = (Player)nextTarget.Pawn;
	}

	public override InventoryComponent GetObservedInventory()
	{
		return Target?.Inventory;
	}

	public override float GetObservedHealth()
	{
		return Target?.Health ?? base.GetObservedHealth();
	}

	public override Team GetObservedTeam()
	{
		return Target?.Team ?? base.GetObservedTeam();
	}

	public override string GetObservedName()
	{
		var characterName = Target?.CharacterName ?? "";
		return string.IsNullOrWhiteSpace( characterName ) ? Target?.Client.Name ?? "Unknown" : characterName;
	}

	public override string GetObservedColour()
	{
		return Target?.HexColor ?? base.GetObservedColour();
	}
}
