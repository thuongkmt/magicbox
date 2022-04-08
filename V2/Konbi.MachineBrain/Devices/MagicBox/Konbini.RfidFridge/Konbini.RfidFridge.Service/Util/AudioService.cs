using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Service.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Util
{
    public class AudioService
    {
        private LogService LogService;

        private System.Media.SoundPlayer player;

        public AudioService(LogService logService)
        {
            this.LogService = logService;
        }


        public void StopSound()
        {
            player.Stop();
        }

        public void PlaySound(string fileName)
        {
            try
            {
                Task.Run(() =>
                {
                    var fullFileName = $"{RfidFridgeSetting.CustomerUI.SoundFolder}\\{fileName}";
                    //LogService.Debug("Playing sound: " + fullFileName);
                    if (File.Exists(fullFileName))
                    {
                        player = new System.Media.SoundPlayer(fullFileName);

                        player.Play();
                    }
                    else
                    {
                        LogService.LogInfo($"Audio file {fullFileName} does not exist!!!");
                    }
                });
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }
    }
}
