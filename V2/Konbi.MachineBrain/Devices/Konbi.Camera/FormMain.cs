namespace Konbi.Camera
{
    using KonbiBrain.Common.Messages;
    using KonbiBrain.Common.Messages.Camera;
    using KonbiBrain.Common.Messages.Payment;
    using KonbiBrain.Common.Services;
    using KonbiBrain.Messages;
    using Konbini.Messages.Enums;
    using Newtonsoft.Json;
    using NsqSharp;
    using System;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    public partial class FormMain : Form, INsqHandler
    {
        private readonly NsqMessageProducerService nsqProducerService;
        private readonly NsqMessageConsumerService nsqConsumerService;
        private readonly LogService logService;
        private const string tempImgFolder = "C:\\TempImgFolder";
        private const string jpgExtenstion = ".jpg";
        private string beginImage;
        private string endImage;

        public FormMain()
        {
            InitializeComponent();
            nsqProducerService = new NsqMessageProducerService();
            nsqConsumerService = new NsqMessageConsumerService(NsqTopics.CAMERA_REQUEST_TOPIC, this);
            logService = new LogService();
            if(!Directory.Exists(tempImgFolder))
            {
                Directory.CreateDirectory(tempImgFolder);
            }
        }

        /// <summary>
        /// The helper class for a combo box item.
        /// </summary>
        

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (WebCameraId camera in webCameraControl1.GetVideoCaptureDevices())
            {
                cbCamera.Items.Add(new ComboBoxItem (camera));
            }

            if (cbCamera.Items.Count > 0)
            {
                cbCamera.SelectedItem = cbCamera.Items[0];
                StartCapture();
            }
            cbCompress.Items.Add(new CompressItem { Name = "10%", Value = 10L });
            cbCompress.Items.Add(new CompressItem { Name = "20%", Value = 20L });
            cbCompress.Items.Add(new CompressItem { Name = "30%", Value = 30L });
            cbCompress.Items.Add(new CompressItem { Name = "40%", Value = 40L });
            cbCompress.Items.Add(new CompressItem { Name = "50%", Value = 50L });
            cbCompress.Items.Add(new CompressItem { Name = "60%", Value = 60L });
            cbCompress.Items.Add(new CompressItem { Name = "70%", Value = 70L });
            cbCompress.Items.Add(new CompressItem { Name = "80%", Value = 80L });
            cbCompress.Items.Add(new CompressItem { Name = "90%", Value = 90L });

            cbCompress.SelectedItem = cbCompress.Items[2];
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            StartCapture();
        }
        private void StartCapture()
        {
            ComboBoxItem i = (ComboBoxItem)cbCamera.SelectedItem;

            try
            {
                webCameraControl1.StartCapture(i.Id);
            }
            finally
            {
                UpdateButtons();
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            webCameraControl1.StopCapture();

            UpdateButtons();
        }

        private void imageButton_Click(object sender, EventArgs e)
        {
            if (cbCamera.Items == null || cbCamera.Items.Count == 0)
            {
                return;
            }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JPEG Image|*.jpg";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.DefaultExt = ".jpg";
            saveFileDialog1.FileName = "Test";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var currentImg = webCameraControl1.GetCurrentImage();
                var compress = ((CompressItem)cbCompress.SelectedItem).Value;
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compress);
                currentImg.Save(saveFileDialog1.FileName, GetEncoder(ImageFormat.Jpeg), encoderParameters);
            }
        }

        private void SaveImage(bool isBegin = true)
        {
            try
            {
                if(cbCamera.Items == null || cbCamera.Items.Count == 0)
                {
                    return;
                }
                var imgName = DateTime.Now.Ticks.ToString() + jpgExtenstion;
                var imgPath = Path.Combine(tempImgFolder, imgName);
                var currentImg = webCameraControl1.GetCurrentImage();
                var compress = 50L;
                Invoke(new Action(() => {
                    compress = ((CompressItem)cbCompress.SelectedItem).Value;
                }));
                
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compress);
                currentImg.Save(imgPath, GetEncoder(ImageFormat.Jpeg), encoderParameters);

                if(isBegin)
                {
                    beginImage = imgPath;
                }
                else
                {
                    endImage = imgPath;
                }
                logService?.LogInfo($"{imgPath} saved");
            }
            catch (Exception ex)
            {
                logService?.LogException(ex.Message);
                logService?.LogException(ex);
            }
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            startButton.Enabled = cbCamera.SelectedItem != null;
            stopButton.Enabled = webCameraControl1.IsCapturing;
            imageButton.Enabled = webCameraControl1.IsCapturing;
        }

        public void HandleMessage(IMessage message)
        {
            var msg = Encoding.UTF8.GetString(message.Body);
            var cmd = JsonConvert.DeserializeObject<NsqCameraRequestCommand>(msg);
            if (cmd == null)
                return;

            if (cmd.Command == UniversalCommandConstants.CameraRequest)
            {
                if(cmd.IsPaymentStart)
                {
                    SaveImage();
                }
                else
                {
                    SaveImage(false);
                    var responseCmd = new NsqCameraResponseCommand()
                    {
                        BeginImage = beginImage,
                        EndImage = endImage
                    };
                    nsqProducerService.SendNsqCommand(NsqTopics.CAMERA_RESPONSE_TOPIC, responseCmd);
                    beginImage = string.Empty;
                    endImage = string.Empty;
                }
            }
        }

        public void LogFailedMessage(IMessage message)
        {
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            nsqProducerService?.Dispose();
        }
    }
}
