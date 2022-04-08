using Konbini.RfidFridge.Service.Core;
using Konbini.RfidFridge.Service.Util;
using MagicCashlessPayment.Core;
using MagicCashlessPayment.Core.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicCashlessPayment.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var logService = new LogService();
            logService.Init();

            var app = new TestApplication(logService, null, null);

            Console.Write("Comport: ");
            var comport = Console.ReadLine();
            app.Connect(comport);


            SIDA: 
            Console.Write("Amount: ");
            var amount = Console.ReadLine();
            app.Test(int.Parse(amount), IucSerialPortInterfaceV2.WalletLabel.DBSMAXDEMO);


            while (true)
            {
                //app.Validate();
                var data = Console.ReadLine();
                if (data == "1")
                {
                    goto SIDA;
                }
            }

        }
    }
}
