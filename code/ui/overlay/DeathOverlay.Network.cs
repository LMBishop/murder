using Sandbox;

namespace MurderGame;

public partial class DeathOverlay
{
    [ClientRpc]
    public static void Show( )
    {
        if (Instance != null) Instance.ShowOverlay = true;
    }

    [ClientRpc]
    public static void Hide()
    {
        if (Instance != null) Instance.ShowOverlay = false;
    }
}
