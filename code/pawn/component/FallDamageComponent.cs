using Sandbox;

namespace MurderGame;

public class FallDamageComponent : EntityComponent<Player>, ISingletonComponent
{
	private const float LethalFallSpeed = 1024;
	private const float SafeFallSpeed = 580;
	private const float DamageForSpeed = 100 / (LethalFallSpeed - SafeFallSpeed); // damage per unit per second.
	private float PreviousZVelocity;

	public void Simulate( IClient cl )
	{
		var FallSpeed = -PreviousZVelocity;
		if ( FallSpeed > SafeFallSpeed * Entity.Scale && Entity.GroundEntity != null )
		{
			var FallDamage = (FallSpeed - SafeFallSpeed * Entity.Scale) * (DamageForSpeed * Entity.Scale);
			var info = DamageInfo.Generic( FallDamage ).WithTag( "fall" );
			Entity.TakeDamage( info );
			Entity.PlaySound( "fall" );
		}

		PreviousZVelocity = Entity.Velocity.z;
	}
}
