namespace KonbiCloud.Common
{
    public enum IoTHubCommands
    {
        CheckMachineOnline = 12001,
        D2CUpdateMachineStatus = 12002,
        D2CVMCStatus = 12003,
        D2CHardwareDiagnostic = 12004,
        C2DSetPriceOnLoadout = 12005,
        C2DDowloadLog = 12006,
        C2DDeployNewVersion = 12007,
    }
}