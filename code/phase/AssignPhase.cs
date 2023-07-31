using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace MurderGame;

public class AssignPhase : BasePhase
{
	public override string Title => "Assigning teams";
	public int TicksElapsed;
	
	private List<string> NatoNames = new()
	{
		"Alpha", 
		"Bravo", 
		"Charlie",
		"Delta",
		"Echo",
		"Foxtrot",
		"Golf",
		"Hotel",
		"India",
		"Juliet",
		"Kilo",
		"Lima",
		"Mike",
		"November",
		"Oscar",
		"Papa",
		"Quebec",
		"Romeo",
		"Sierra",
		"Tango",
		"Uniform",
		"Victor",
		"Whiskey",
		"X-Ray",
		"Yankee",
		"Zulu"
	};
	
	private List<string> HexColours = new()
	{
		"#0074D9", // blue
		"#7FDBFF", // aqua
		"#39CCCC", // teal
		"#F012BE", // fuchsia
		"#FF4136", // red
		"#FF851B", // orange
		"#FFDC00", // yellow
		"#3D9970", // olive
		"#2ECC40", // lime
		"#01FF70"  // green
	};

	public override void Activate()
	{
		// cleanup -- start
		foreach (var entity in Entity.All.OfType<DroppedWeapon>())
		{
			entity.Delete();
		}
		// cleanup -- end

		var detectivesNeeded = 1;
		var murderersNeeded = 1;
		
		Random random = new();
		
		var spawnPoints = Entity.All.OfType<SpawnPoint>().OrderBy( _ => random.Next() ).ToList();
		var clients = Game.Clients.ToList().OrderBy( _ => random.Next() );
		var natoNamesRemaining = new List<string>(NatoNames.OrderBy( _ => random.Next() ));
		var coloursRemaining = new List<string>(HexColours.OrderBy( _ => random.Next() ));

		foreach ( var client in clients )
		{
			if (client.Pawn != null)
			{
				((Player) client.Pawn).Cleanup();
				client.Pawn.Delete();
			}
			
			Player pawn = new();
			client.Pawn = pawn;
			
			if (spawnPoints.Count == 0)
			{
				ChatBox.Say( "Could not spawn " + client.Name + " as there are not enough spawn points." );
				pawn.Team = Team.Spectator;
				continue;
			}
			pawn.DressFromClient( client );
			
			// re-use names and colours if needed
			if (natoNamesRemaining.Count == 0)
			{
				natoNamesRemaining = new List<string>(NatoNames);
			}
			if (coloursRemaining.Count == 0)
			{
				coloursRemaining = new List<string>(HexColours);
			}

			// assign team
			if (murderersNeeded > 0)
			{
				pawn.Team = Team.Murderer;
				--murderersNeeded;
			}
			else if (detectivesNeeded > 0)
			{
				pawn.Team = Team.Detective;
				--detectivesNeeded;
			}
			else
			{
				pawn.Team = Team.Bystander;
			}
			Log.Info( "Assigning " + client.Name + " to team " + pawn.GetTeamName() );

			// position pawn
			var spawnPoint = spawnPoints[0];
			spawnPoints.RemoveAt( 0 );
			var tx = spawnPoint.Transform;
			tx.Position += Vector3.Up * 10.0f;
			pawn.Transform = tx;
			
			// assign nato name
			var natoName = natoNamesRemaining[0];
			natoNamesRemaining.RemoveAt( 0 );
			pawn.CharacterName = natoName;
			
			// assign nato name
			var hexColour = coloursRemaining[0];
			coloursRemaining.RemoveAt( 0 );
			pawn.HexColor = hexColour;

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
		if ( TimeLeft == -1 || TicksElapsed % Game.TickRate != 0 || --base.TimeLeft != 0 )
		{
			return;
		}

		IsFinished = true;
		NextPhase = new PlayPhase();
	}

}
