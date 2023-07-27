using Sandbox;

public partial class Ragdoll : AnimatedEntity
{
	public Ragdoll()
	{
		Tags.Add( "ragdoll" );
		PhysicsEnabled = true;
		UsePhysicsCollision = true;
		EnableSelfCollisions = true;
		EnableSolidCollisions = true;
	}
}
