﻿using Sandbox;

namespace MurderGame;

public partial class BlindedOverlay
{
	[ClientRpc]
	public static void Show()
	{
		Instance.SetClass( "hidden", false );
		Instance.ShowOverlay = true;
	}

	[ClientRpc]
	public static void Hide()
	{
		Instance.SetClass( "hidden", true );
		Instance.ShowOverlay = false;
	}
}
