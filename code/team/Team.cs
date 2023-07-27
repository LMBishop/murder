using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurderGame;

public enum Team : ushort
{
	Spectator = 0,
	Murderer = 1,
	Detective = 2,
	Bystander = 3
}

// why are c# enums so bad
public static class TeamOperations
{

	public static string GetTeamName(Team team)
	{
		return team switch
		{
			Team.Detective => "Detective",
			Team.Murderer => "Murderer",
			Team.Bystander => "Bystander",
			Team.Spectator => "Spectator",
			_ => "None",
		};
	}

	public static string GetTeamColour(Team team)
	{
		return team switch
		{
			Team.Detective => "#33A0FF",
			Team.Murderer => "#FF4136",
			Team.Bystander => "#33A0FF",
			_ => "#AAAAAA",
		};
	}

	public static string GetTeamDescription(Team team)
	{
		return team switch
		{
			Team.Detective => "There is a murderer on the loose! Find out who they are and shoot them before they kill everybody else.",
			Team.Murderer => "Kill everybody else in time and avoid detection. At least one other player is armed.",
			Team.Bystander => "There is a murderer on the loose! Avoid getting killed and work with others to establish who the murderer is.",
			_ => "None",
		};
	}

	public static bool CanSprint(Team team)
	{
		return team switch
		{
			Team.Murderer => true,
			_ => false,
		};
	}
	
	public static void GiveLoadouts(Player pawn)
	{
		pawn.Inventory.Clear();
		
		switch (pawn.CurrentTeam)
		{
			case Team.Detective:
				GiveDetectiveWeapon(pawn); break;
			case Team.Murderer:
				GiveMurdererWeapon(pawn); break;
			default: break;
		}
	}

	private static void GiveDetectiveWeapon( Player pawn )
	{
		Pistol pistol = new()
		{
			Ammo = 1
		};
		pawn.Inventory.SetPrimaryWeapon( pistol );
	}

	private static void GiveMurdererWeapon(Player pawn)
	{
		pawn.Inventory.SetPrimaryWeapon( new Knife() );
	}
}
