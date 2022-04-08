using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BillAcceptorBrain
{
    class Program
    {
        static void Main(string[] args)
        {
            var comPort = ConfigurationManager.AppSettings["BillAcceptorPort"];
            var svc=new BillAcceptorService(comPort);
            Console.WriteLine($"Bill Acceptor started at {comPort}");


            while (true)
            {
                var readline = Console.ReadLine();
                if (readline == "en")
                {
                    svc.Enable();
                }

                if (readline == "ds")
                {
                    svc.Disable();
                }

                if (readline == "reset")
                {
                    svc.Reset();
                }

                if (readline == "exit")
                {
                    svc.Disable();
                    break;
                }
            }
        }
    }
}
