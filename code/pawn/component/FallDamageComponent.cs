using Sandbox;

namespace MurderGame;

public partial class FallDamageComponent : EntityComponent<Player>, ISingletonComponent
{
	float PreviousZVelocity = 0;
	const float LethalFallSpeed = 1024;
	const float SafeFallSpeed = 580;
	const float DamageForSpeed = (float)100 / (LethalFallSpeed - SafeFallSpeed); // damage per unit per second.
	public void Simulate( IClient cl )
	{
		var FallSpeed = -PreviousZVelocity;
		if ( FallSpeed > (SafeFallSpeed * Entity.Scale) && Entity.GroundEntity != null )
		{
			var FallDamage = (FallSpeed - (SafeFallSpeed * Entity.Scale)) * (DamageForSpeed * Entity.Scale);
			var info = DamageInfo.Generic( FallDamage ).WithTag( "fall" );
			Entity.TakeDamage( info );
			Entity.PlaySound( "fall" );
		}
		PreviousZVelocity = Entity.Velocity.z;
	}
}
