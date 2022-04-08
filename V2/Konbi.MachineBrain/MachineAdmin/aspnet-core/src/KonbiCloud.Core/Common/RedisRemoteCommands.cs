namespace KonbiCloud.Common
{
    public enum RedisRemoteCommands
    {
        UNDEFINED=11000,
        DisconnectReconnectVmc = 11001,
        RemoteCheckVmcElevator = 11002,
        RemoteDispense = 11003,
        RemoteMoveElevatorToLevel = 11004,
        RestartPC = 11005,
        SetOverleyCustomerTemporalMessage = 11006
    }
}
