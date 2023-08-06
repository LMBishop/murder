using Sandbox;

namespace MurderGame;

public class EndPhase : BasePhase
{
	public int TicksElapsed;
	public override string Title => "Game over";

	public override void Activate()
	{
		TimeLeft = 7;
	}

	public override void Tick()
	{
		++TicksElapsed;
		if ( TimeLeft != -1 && TicksElapsed % Game.TickRate == 0 && --TimeLeft == 0 )
		{
			NextPhase = new WaitPhase { CountIn = false };
			IsFinished = true;
		}
	}
}
