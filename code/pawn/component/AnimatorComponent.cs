using Sandbox;

namespace MurderGame;

public class AnimatorComponent : EntityComponent<Player>, ISingletonComponent
{
	public void Simulate()
	{
		var helper = new CitizenAnimationHelper( Entity );
		var player = Entity;

		helper.WithVelocity( Entity.Velocity );
		helper.WithLookAt( Entity.EyePosition + Entity.EyeRotation.Forward * 100 );
		helper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		helper.IsGrounded = Entity.GroundEntity.IsValid();
		helper.DuckLevel = MathX.Lerp( helper.DuckLevel, player.Controller.HasTag( "ducked" ) ? 1 : 0,
			Time.Delta * 10.0f );
		helper.VoiceLevel = Game.IsClient && player.Client.IsValid()
			? player.Client.Voice.LastHeard < 0.5f ? player.Client.Voice.CurrentLevel : 0.0f
			: 0.0f;
		helper.IsClimbing = player.Controller.HasTag( "climbing" );
		helper.IsSwimming = player.GetWaterLevel() >= 0.5f;

		if ( Entity.Controller.HasEvent( "jump" ) )
		{
			helper.TriggerJump();
		}
	}
}
