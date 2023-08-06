using Sandbox;

namespace MurderGame;

public abstract partial class BasePhase : BaseNetworkable
{
	public virtual string Title => "Name";

	[Net] public int TimeLeft { get; set; } = -1;

	public BasePhase NextPhase { get; set; }

	public bool IsFinished { get; set; }

	public abstract void Tick();

	public virtual void Activate() { }

	public virtual void Deactivate() { }

	public virtual void HandleClientJoin( ClientJoinedEvent e ) { }
}
