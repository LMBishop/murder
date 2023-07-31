using Sandbox;

namespace MurderGame;

public partial class Player
{
	[Net] public Team Team { get; set; }
	
	[Net] public string CharacterName { get; set; }

	[Net] public string HexColor { get; set; }

	public string GetTeamName()
	{
		return TeamOperations.GetTeamName( Team );
	}
}
