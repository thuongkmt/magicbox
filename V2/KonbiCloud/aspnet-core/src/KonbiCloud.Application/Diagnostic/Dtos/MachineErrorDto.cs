using System;

namespace KonbiCloud.Diagnostic.Dtos
{
    public class MachineErrorDto
    {
        public Guid MachineId { get; set; }
        public string MachineName { get; set; }
        public string MachineErrorCode { get; set; }

        public string Message { get; set; }
        public string Solution { get; set; }
        public string Time { get; set; }
    }

    public class VendingStatusDto
    {
        public Guid MachineID { get; set; }
        public string VmcLevel { get; set; }
        public bool VmcOk { get; set; }
        public bool IucOk { get; set; }
        public bool CykloneOk { get; set; }
        public bool MdbOk { get; set; }
        public bool IsSynced { get; set; }
        public float Temperature { get; set; }

        public string SnapshotUrl { get; set; }
    }
}