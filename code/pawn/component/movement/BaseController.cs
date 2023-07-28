using Sandbox;

namespace MurderGame;

//TODO make spectatro a controller
public class BaseController : EntityComponent<Player>
{
	public Player Player { get; set; }
}
