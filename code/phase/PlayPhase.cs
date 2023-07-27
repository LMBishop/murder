using Sandbox;
using Sandbox.UI;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace MurderGame;

public class PlayPhase : BasePhase
{
	public override string Title => "Play";
	public IDictionary<Entity, int> Blinded = new Dictionary<Entity, int>();
	public int TicksElapsed;
	private string MurdererNames { get; set; }

	public override void Activate()
	{
		base.TimeLeft = MurderGame.RoundTime;
		Event.Register(this);
		foreach ( var client in Game.Clients )
		{
			if ( client.Pawn is Player pawn )
			{
				if (pawn.CurrentTeam != Team.Spectator)
				{
					pawn.Respawn();
					TeamOperations.GiveLoadouts( pawn );
				}
			}
		}
		MurdererNames = string.Join( ',', Game.Clients.Where( c => ((Player)c.Pawn).CurrentTeam == Team.Murderer ).Select(c => c.Name));
	}

	public override void Deactivate()
	{
		base.TimeLeft = MurderGame.RoundTime;
		Event.Unregister(this);
		foreach(var item in Blinded)
		{
			ClearDebuffs( item.Key );
		}
		Blinded.Clear();
	}

	public void ClearDebuffs(Entity entity )
	{
		Log.Info( "Removing blind from " + entity.Name );
		BlindedOverlay.Hide( To.Single( entity ) );
		if (entity is Player pawn && pawn.IsValid() )
		{
			if (pawn.Controller != null) pawn.Controller.SpeedMultiplier = 1;
			if (pawn.Inventory != null) pawn.Inventory.AllowPickup = true;
		}

	}

	public override void Tick()
	{
		++TicksElapsed;
		if (base.TimeLeft != -1 && TicksElapsed % Game.TickRate == 0 && --base.TimeLeft == 0)
		{
			TriggerEndOfGame();
			return;
		}
		bool bystandersAlive = Game.Clients.Any(c =>((Player)c.Pawn).CurrentTeam == Team.Bystander || ((Player)c.Pawn).CurrentTeam == Team.Detective);
		bool murderersAlive = Game.Clients.Any(c =>((Player)c.Pawn).CurrentTeam == Team.Murderer);
		if (!bystandersAlive || !murderersAlive)
		{
			TriggerEndOfGame();
		}

		foreach(var item in Blinded)
		{
			var blindLeft = item.Value - 1;
			if (blindLeft < 0)
			{
				Blinded.Remove( item.Key );
				ClearDebuffs( item.Key );
				Log.Info( "Removing blind from " + item.Key.Name );
			}
			else
			{
				Blinded[item.Key] = blindLeft;
			}
		}
	}

	public void TriggerEndOfGame()
	{
		bool bystandersWin = Game.Clients.Any(c =>((Player)c.Pawn).CurrentTeam == Team.Bystander || ((Player)c.Pawn).CurrentTeam == Team.Detective);
		ChatBox.Say( (bystandersWin ? "Bystanders" : "Murderers") +" win! The murderers were: " + MurdererNames );
		base.NextPhase = new EndPhase();
		base.IsFinished = true;
	}

	[MurderEvent.Kill]
	public void OnKill(Entity killer, Entity victim)
	{
		if (killer == null || killer is not Player || victim == null || victim is not Player )
		{
			return;
		}
		Player victimPlayer = (Player)victim;
		Player killerPlayer = (Player)killer;
		Team victimTeam = victimPlayer.CurrentTeam;
		Team killerTeam = killerPlayer.CurrentTeam;
		victimPlayer.CurrentTeam = Team.Spectator;

		Log.Info( victimPlayer + " died to " + killerPlayer );
		if (victimTeam != Team.Murderer && killerTeam != Team.Murderer) 
		{
			Log.Info( killerPlayer + " shot a bystander");
			ChatBox.Say( killerPlayer.Client.Name + " killed an innocent bystander" );
			BlindedOverlay.Show( To.Single( killer ) );
			if (killerPlayer.Controller != null) killerPlayer.Controller.SpeedMultiplier = 0.3f;
			if (killerPlayer.Inventory != null)
			{
			Log.Info( killerPlayer + "bonk");
				killerPlayer.Inventory.AllowPickup = false;
				killerPlayer.Inventory.SpillContents(killerPlayer.EyePosition, killerPlayer.AimRay.Forward);
			}

			Blinded[killer] = 20 * Game.TickRate;
		}
		else if (victimTeam == Team.Murderer )
		{
			Log.Info( killerPlayer + " killed a murderer");
			ChatBox.Say( killerPlayer.Client.Name + " killed a murderer" );
		}
	}

}
