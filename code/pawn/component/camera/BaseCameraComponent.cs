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
		return Entity.Inventory;
	}
	
	public virtual float GetObservedHealth()
	{
		return Entity.Health;
	}

	public virtual Team GetObservedTeam()
	{
		return Entity.Team;	
	}
	
	public virtual string GetObservedName()
	{
		var characterName = Entity.CharacterName;
		return string.IsNullOrWhiteSpace( characterName ) ? Entity.Client.Name : characterName;
	}
	
	public virtual string GetObservedColour()
	{
		return Entity.HexColor;	
	}
}
