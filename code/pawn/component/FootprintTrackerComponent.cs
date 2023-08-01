using System.Linq;
using Sandbox;

namespace MurderGame;

public class FootprintTrackerComponent : EntityComponent<Player>, ISingletonComponent
{ 
	private TimeSince TimeSinceFootstep = 0;
	private bool FootstepLeft = true;

	public void Simulate( IClient cl )
	{
		if (!Game.IsClient || TimeSinceFootstep < 0.25) return;
		TimeSinceFootstep = 0;
		FootstepLeft = !FootstepLeft;
		
		var bystanders = Game.Clients.Where(c => (c.Pawn as Player)?.Team is Team.Bystander or Team.Detective);
		
		foreach (var bystander in bystanders)
		{
			if (bystander.Pawn is not Player player) continue;
			if (player.Velocity.Length < 1) continue;
			var start = player.Position + Vector3.Up;
			var end = start + Vector3.Down * 20;
			
			var tr = Trace.Ray( start, end )
				.Size( 2)
				.WithAnyTags( "solid" )
				.Ignore( Entity )
				.Run();

			if ( !tr.Hit )
			{
				continue;
			}

			var material = FootstepLeft
				? "materials/left_shoe_footprint.vmat"
				: "materials/right_shoe_footprint.vmat";
			var _ = new Footprint
			{
				SpriteMaterial = Material.Load(material),
				SpriteScale = 24f,
				Position = player.Position + (Vector3.Up * 1f),
				Rotation = Rotation.LookAt(player.Velocity, tr.Normal).RotateAroundAxis( tr.Normal, 270 ),
				Color = player.Color
			};
		}
	}
	
}
