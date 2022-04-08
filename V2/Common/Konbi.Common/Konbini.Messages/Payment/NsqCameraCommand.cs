namespace KonbiBrain.Common.Messages.Camera
{
    public class NsqCameraRequestCommand
    {
        public NsqCameraRequestCommand()
        {
            Command = UniversalCommandConstants.CameraRequest;
        }
        public string Command { get; set; }
        public bool IsPaymentStart { get; set; }
    }

    public class NsqCameraResponseCommand
    {
        public NsqCameraResponseCommand()
        {
            Command = UniversalCommandConstants.CameraResponse;
        }
        public string Command { get; set; }
        public string BeginImage { get; set; }
        public string EndImage { get; set; }
    }
}
