using System;
using System.ComponentModel;
using MachineDll;

namespace TemplateBrain
{
    public class ChillerMachine
    {
        private Refrigerator machine;
        
        public ChillerMachine()
        {
            machine = new Refrigerator();
          
        }

       
        /// <summary>
        /// Starts the connectivity to the vending machine.
        /// </summary>
        /// <param name="port">Serial port of the vending machine, e.g. "COM10".</param>
        public void StartRefrigerator(string port)
        {
            // 10 = number of tray columns specific to the vending machine.
            machine.Start((ECOM)Enum.Parse(typeof(ECOM), port));
        }

        /// <summary>
        /// Stops the connectivity to the vending machine.
        /// </summary>
        public void StopRefrigerator()
        {
            machine.Close();
        }

        public void Open()
        {
            //if (Properties.Settings.Default.IsChiller)
                machine.设置运行模式(ERunModel.制冷);
            //else 
            //{
            //    machine.设置运行模式(ERunModel.加热);
            //}
        }

        public void Close()
        {
            //if(Properties.Settings.Default.IsChiller)
                machine.设置运行模式(ERunModel.停机);
            //else
            //{
            //    machine.设置运行模式(ERunModel.停机);
            //}
        }

        public void SetTemperature(double temperature)
        {
            if (temperature == 0) return;//only set temperature great than 0
            Open();
            
            //if (Properties.Settings.Default.IsChiller)
                machine.设置制冷控制温度(Convert.ToInt32(temperature));
            //else
            //{
            //    //this.m_Master.设置加热控制温度(this.txtS3.Value);
            //    this.machine.设置加热控制温度(Convert.ToInt32(temperature));
            //}
        }

        public double GetCurrentTemperature()
        {
            
            //if (Properties.Settings.Default.IsChiller)
                return machine.温度[0];
            //else
            //{
            //    return machine.温度[3];
            //}
        }

        public System.Windows.Forms.UserControl GetHeatControl()
        {
            
            return machine.GetHeatControl();
        }

        public System.Windows.Forms.UserControl GetControlEn()
        {
            return machine.GetControlEn();
        }
    }
}
