using System;
using System.Linq;
using Sandbox;

namespace MurderGame;

public class WaitPhase : BasePhase
{
	public override string Title => "Waiting for players";
	public bool CountIn { get; set; }
	public int TicksElapsed { get; set; }

	private bool _isCountDown { get; set; }

	public override void Tick()
	{
		if ( Game.Clients.Count >= MurderGame.MinPlayers )
		{
			if ( !CountIn || (_isCountDown && ++TicksElapsed % Game.TickRate == 0 && --TimeLeft == 0) )
			{
				NextPhase = new AssignPhase();
				IsFinished = true;
				return;
			}

			if ( CountIn && !_isCountDown )
			{
				_isCountDown = true;
				TimeLeft = 10;
			}
		}
		else if ( CountIn && _isCountDown )
		{
			_isCountDown = false;
			TimeLeft = -1;
		}

		foreach ( var client in Game.Clients )
		{
			if ( client.Pawn == null )
			{
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

				pawn.Spawn();
				RespawnPlayer( pawn );
			}
			else
			{
				var pawn = (Player)client.Pawn;
				if ( pawn.LifeState == LifeState.Dead && pawn.TimeSinceDeath > 5 )
				{
					RespawnPlayer( pawn );
				}
			}
		}
	}

	private void RespawnPlayer( Player pawn )
	{
		pawn.Team = Team.Spectator;
		pawn.DressFromClient( pawn.Client );
		pawn.Respawn();
	}

	public override void HandleClientJoin( ClientJoinedEvent e )
	{
	}
}
