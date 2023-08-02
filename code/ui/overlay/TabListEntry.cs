using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace MurderGame;

public class TabListEntry : Panel
{
	
	public IClient Client;

	public Label PlayerName;
	public Label Ping;
	
	RealTimeSince TimeSinceUpdate = 0;
	
	public TabListEntry()
	{
		AddClass( "entry" );

		PlayerName = Add.Label( "PlayerName", "name" );
		Ping = Add.Label( "", "ping" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible )
			return;

		if ( !Client.IsValid() )
			return;

		if ( TimeSinceUpdate < 0.1f )
			return;

		TimeSinceUpdate = 0;
		UpdateData();
	}
	
	public virtual void UpdateData()
	{
		PlayerName.Text = Client.Name;
		Ping.Text = Client.Ping.ToString();
	}
	
	public virtual void UpdateFrom( IClient client )
	{
		Client = client;
		UpdateData();
	}
	
}
