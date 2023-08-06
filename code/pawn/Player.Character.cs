using Sandbox;

namespace MurderGame;

public partial class Player
{
	[Net] public Team Team { get; set; }

	[Net] public string CharacterName { get; set; }

	[Net] public Color Color { get; set; } = Color.White;

	public string HexColor => Color.Hex;

	public string GetTeamName()
	{
		return TeamOperations.GetTeamName( Team );
	}
}
