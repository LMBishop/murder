using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurderGame;

public class WaitPhase : BasePhase
{
	public override string Title => "Waiting for players";
	public bool CountIn { get; set; }
	public int TicksElapsed { get; set; }

	private bool _isCountDown { get; set; }

	public override void Tick()
	{
		if (Game.Clients.Count >= MurderGame.MinPlayers)
		{
			if (!CountIn || (_isCountDown && ++TicksElapsed % Game.TickRate == 0 && --base.TimeLeft == 0))
			{
				base.NextPhase = new AssignPhase();
				base.IsFinished = true;
			}
			else if (CountIn && !_isCountDown)
			{
				_isCountDown = true;
				base.TimeLeft = 10;
			}
		} else if (CountIn && _isCountDown)
		{
			_isCountDown = false;
			base.TimeLeft = -1;
		}
	}

	public override void HandleClientJoin( ClientJoinedEvent e )
	{
		
	}
}
