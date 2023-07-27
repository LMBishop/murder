using Sandbox;
using System.Linq;

namespace MurderGame;

public class EndPhase : BasePhase
{
	public override string Title => "Game over";
	public int TicksElapsed;

	public override void Activate()
	{
		base.TimeLeft = 7;
	}

	public override void Tick()
	{
		++TicksElapsed;
		if (base.TimeLeft != -1 && TicksElapsed % Game.TickRate == 0 && --base.TimeLeft == 0)
		{
			base.NextPhase = new WaitPhase() { CountIn = false };
			base.IsFinished = true;
			return;
		}
	}
}
