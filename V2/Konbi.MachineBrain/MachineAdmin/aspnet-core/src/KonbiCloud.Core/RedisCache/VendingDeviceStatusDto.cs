namespace KonbiCloud.RedisCache
{
    public class VendingDeviceStatusDto
    {
        public string Name { get; set; }
        public bool IsConnected { get; set; }
        public string State { get; set; }
        public string[] ErrorMessages { get; set; }
    }
}