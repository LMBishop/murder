using Sandbox;

namespace MurderGame;

public static class MurderEvent
{
    public const string Kill = "kill";

    public class KillAttribute : EventAttribute
    {
        public KillAttribute() : base( Kill ) { }
    }
}
