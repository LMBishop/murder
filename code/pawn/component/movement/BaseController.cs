using Sandbox;

namespace MurderGame;

//TODO make spectatro a controller
public abstract class BaseController
{
	public virtual float SpeedMultiplier { get; set; } = 1;

	public abstract void Simulate(Player player);

	public abstract bool HasEvent(string eventName);

}
