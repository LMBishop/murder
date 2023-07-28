using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MurderGame;

public class AssignPhase : BasePhase
{
	public override string Title => "Assigning teams";
	public int TicksElapsed;

	public override void Activate()
	{
		foreach (var entity in Entity.All.OfType<DroppedWeapon>())
		{
			entity.Delete();
		}

		var detectivesNeeded = 1;
		var murderersNeeded = 1;

		Random random = new();
		List<SpawnPoint> spawnpoints = Entity.All.OfType<SpawnPoint>().OrderBy( _ => random.Next() ).ToList();
		var clients = Game.Clients.ToList();
		foreach ( int i in Enumerable.Range( 0, clients.Count ).OrderBy( _ => random.Next() ) )
		{
			var client = clients[i];
			if (client.Pawn != null)
			{
				((Player) client.Pawn).Cleanup();
				client.Pawn.Delete();
			}
			Player pawn = new();
			client.Pawn = pawn;
			if (spawnpoints.Count == 0)
			{
				ChatBox.Say( "Could not spawn " + client.Name + " as there are not enough spawn points." );
				pawn.CurrentTeam = Team.Spectator;
				continue;
			}
			pawn.DressFromClient( client );

			if (murderersNeeded > 0)
			{
				pawn.CurrentTeam = Team.Murderer;
				--murderersNeeded;
			}
			else if (detectivesNeeded > 0)
			{
				pawn.CurrentTeam = Team.Detective;
				--detectivesNeeded;
			}
			else
			{
				pawn.CurrentTeam = Team.Bystander;
			}
			Log.Info( "Assigning " + client.Name + " to team " + TeamOperations.GetTeamName( pawn.CurrentTeam ) );

			var spawnpoint = spawnpoints[0];
			spawnpoints.RemoveAt( 0 );
			var tx = spawnpoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 10.0f;
			pawn.Transform = tx;

			RoleOverlay.Show( To.Single( client ) );
		}
		base.TimeLeft = 5;
	}

	public override void Deactivate()
	{
		foreach (var client in Game.Clients)
		{
			RoleOverlay.Hide( To.Single( client ) );
		}
	}

	public override void Tick()
	{
		++TicksElapsed;
		if ( base.TimeLeft != -1 && TicksElapsed % Game.TickRate == 0 && --base.TimeLeft == 0 )
		{
			base.IsFinished = true;
			base.NextPhase = new PlayPhase();
		}
	}

}
