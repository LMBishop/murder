using System;
using Sandbox;

namespace MurderGame;

// It's just a square with a material slapped onto it.
public class Footprint : RenderEntity
{
	private readonly TimeSince TimeSinceCreated = 0;
	public Material SpriteMaterial { get; set; }

	public float SpriteScale { get; set; } = 18f;

	public bool Enabled { get; set; } = true;

	public Color Color { get; set; }

	[GameEvent.Tick.Client]
	public void OnTick()
	{
		if ( !(TimeSinceCreated > Math.Clamp( MurderGame.MaxFootprintTime, 0, 30 )) )
		{
			return;
		}

		Enabled = false;
		Delete();
	}

	public override void DoRender( SceneObject obj )
	{
		if ( !Enabled )
		{
			return;
		}

		// Allow lights to affect the sprite
		Graphics.SetupLighting( obj );
		// Create the vertex buffer for the sprite
		var vb = new VertexBuffer();
		vb.Init( true );

		// Vertex buffers are in local space, so we need the camera position in local space too
		var normal = new Vector3( 0, 0.01f, 100 );
		var w = normal.Cross( Vector3.Down ).Normal;
		var h = normal.Cross( w ).Normal;
		var halfSpriteSize = SpriteScale / 2;

		// Add a single quad to our vertex buffer
		vb.AddQuad( new Ray( default, normal ), halfSpriteSize * w, h * halfSpriteSize );

		Graphics.Attributes.Set( "color", Color );

		// Draw the sprite
		vb.Draw( SpriteMaterial );
	}
}
