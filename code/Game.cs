using System;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace MurderGame;

public partial class MurderGame : GameManager
{
	public MurderGame()
	{
		if ( Game.IsClient )
		{
			Game.RootPanel = new Hud();
		}
	}

	public static MurderGame Instance => Current as MurderGame;


	[ConVar.Server( "mu_min_players", Help = "The minimum number of players required to start a round." )]
	public static int MinPlayers { get; set; } = 2;

	[ConVar.Server( "mu_allow_suicide", Help = "[INOP] Allow players to kill themselves during a round." )]
	public static bool AllowSuicide { get; set; } = true;

	[ConVar.Server( "mu_round_time", Help = "The amount of time, in seconds, in a round." )]
	public static int RoundTime { get; set; } = 600;

	[ConVar.Client( "mu_max_footprint_time",
		Help = "The amount of time, in seconds, footprints are visible for. Max 30 seconds." )]
	public static int MaxFootprintTime { get; set; } = 30;

	[Net] public BasePhase CurrentPhase { get; set; } = new WaitPhase { CountIn = true };

	[GameEvent.Tick.Server]
	public void TickServer()
	{
		CurrentPhase.Tick();

		if ( CurrentPhase.NextPhase != null && CurrentPhase.IsFinished )
		{
			CurrentPhase.Deactivate();
			CurrentPhase = CurrentPhase.NextPhase;
			Log.Info( "Advancing phase to " + CurrentPhase );
			CurrentPhase.Activate();
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Player();
		client.Pawn = pawn;
		pawn.Spawn();

		var spawnpoints = All.OfType<SpawnPoint>();
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f;
			pawn.Transform = tx;
		}

		ChatBox.Say( client.Name + " joined the game" );
	}

	public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( client, reason );

		ChatBox.Say( client.Name + " left the game (" + reason + ")" );
	}
}
