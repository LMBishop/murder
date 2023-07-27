﻿
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MurderGame;

public partial class MurderGame : Sandbox.GameManager
{
	public MurderGame()
	{
		if ( Game.IsClient )
		{
			Game.RootPanel = new Hud();
		}
	//	if ( Game.IsServer )
	//	{
	//		// The packageName should always match org.ident
	//		// In this case, we ask to download gvar.citizen_zombie -> https://asset.party/gvar/citizen_zombie
	//		DownloadAsset( "gvar.citizen_zombie" );
	//	}
	}

	public static MurderGame Instance
	{
		get => Current as MurderGame;
	}


	[ConVar.Server( "mm_min_players", Help = "The minimum number of players required to start a round." )]
	public static int MinPlayers { get; set; } = 2;

	[ConVar.Server( "mm_allow_suicide", Help = "Allow players to kill themselves during a round." )]
	public static bool AllowSuicide { get; set; } = true;

	[ConVar.Server( "mm_round_time", Help = "The amount of time in a round." )]
	public static int RoundTime { get; set; } = 600;

	[Net]
	public BasePhase CurrentPhase { get; set; } = new WaitPhase() { CountIn = true };

	[GameEvent.Tick.Server]
	public void TickServer()
	{
		CurrentPhase.Tick();
		
		if (CurrentPhase.NextPhase != null && CurrentPhase.IsFinished)
		{
			CurrentPhase.Deactivate();
			CurrentPhase = CurrentPhase.NextPhase;
			Log.Info("Advancing phase to " + CurrentPhase.ToString());
			CurrentPhase.Activate();
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Player();
		client.Pawn = pawn;

		var spawnpoints = Entity.All.OfType<SpawnPoint>();
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f;
			pawn.Transform = tx;
		}
	}
}

