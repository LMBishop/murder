using Sandbox;

namespace MurderGame;

public class BaseCameraComponent : EntityComponent<Player>, ISingletonComponent
{

	public virtual void Simulate( IClient cl )
	{

	}
	public virtual void FrameSimulate( IClient cl )
	{

	}
	public virtual void BuildInput()
	{

	}

	public virtual InventoryComponent GetObservedInventory()
	{
		return null;
	}
	
	public virtual float GetObservedHealth()
	{
		return 0;
	}

	public virtual Team GetObservedTeam()
	{
		return Team.Spectator;	
	}
}
