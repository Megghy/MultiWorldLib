namespace MultiWorldLib.Entities
{
    public enum PlayerState
    {
        Disconnect,
        NewConnection,
        InMainServer,
        Switching,
        RequirePassword,
        SyncData,
        InSubServer,
    }
}
