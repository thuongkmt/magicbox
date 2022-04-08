using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Core
{
    public class CameraInterface
    {
        #region Properties
        private int CAMERA_INDEX;
        private string SAVE_PATCH;
        private int IMAGE_WIDTH;
        private int IMAGE_HEIGHT;
        private DirectoryInfo _savePathDirectoryInfo;
        private bool _hasInit;

        private VideoCapture capture;

        #endregion

        #region Services
        public LogService LogService;
        #endregion

        public CameraInterface(LogService logService)
        {
            LogService = logService;
        }

        public void Init(int cameraIndex, string savePath, int imageWidth, int imageHeight)
        {
            try
            {
                CAMERA_INDEX = cameraIndex;
                SAVE_PATCH = savePath;
                IMAGE_WIDTH = imageWidth;
                IMAGE_HEIGHT = imageHeight;
                var initMsg = $"Camera service init: CAMERA_INDEX: {CAMERA_INDEX} | SAVE_PATCH: {SAVE_PATCH} | IMAGE_WIDTH: {IMAGE_WIDTH} | IMAGE_HEIGHT: {IMAGE_HEIGHT}";
                LogService.LogInfo(initMsg);
                LogService.LogCamera(initMsg);
                if (!Directory.Exists(SAVE_PATCH))
                {
                    _savePathDirectoryInfo = Directory.CreateDirectory(SAVE_PATCH);
                    LogService.LogCamera($"Creating folder: {SAVE_PATCH}");
                }
                else
                {
                    _savePathDirectoryInfo = new DirectoryInfo(SAVE_PATCH);
                }

                capture = new VideoCapture(CAMERA_INDEX); //create a camera capture
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, IMAGE_WIDTH);
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, IMAGE_HEIGHT);
                
                _hasInit = true;
            }
            catch (Exception ex)
            {
                LogService.LogCamera(ex.ToString());
            }
        }

        public void CaptureImage(string fileName)
        {
            if (_hasInit)
            {
                Task.Run(() =>
                {
                    LogService.LogCamera($"Saving file: {fileName} to {_savePathDirectoryInfo.FullName}");

                    try
                    {
                        var stopWatch = new Stopwatch();
                        stopWatch.Start();


                        Bitmap image = capture.QueryFrame().Bitmap; //take a picture
                        if (image == null)
                        {
                            LogService.LogCamera("Failed to capture image");
                        }
                        var outputFileName = $"{_savePathDirectoryInfo.FullName}\\{fileName}.jpg";

                        using (MemoryStream memory = new MemoryStream())
                        {
                            using (FileStream fs = new FileStream(outputFileName, FileMode.Create, FileAccess.ReadWrite))
                            {
                                image.Save(memory, ImageFormat.Jpeg);
                                byte[] bytes = memory.ToArray();
                                fs.Write(bytes, 0, bytes.Length);
                                LogService.LogCamera($"Saved, total time: " + stopWatch.Elapsed.TotalMilliseconds);
                                stopWatch.Stop();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogService.LogCamera(ex.ToString());
                    }
                });
            }
            //else
            //{
            //    LogService.LogInfo("Camera has not init");
            //    LogService.LogCamera("Camera has not init");
            //}
        }

    }
}
