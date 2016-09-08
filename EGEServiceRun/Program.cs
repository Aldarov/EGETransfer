using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using EGEServiceRun;

namespace EGEServiceRun
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(EGESupport));
            host.Open();
            Console.WriteLine("Runing");
            string cmd = "";
            while (cmd != "exit")
            {
                cmd = Console.ReadLine();
                if (cmd == "exit")
                {
                    host.Close();
                    Console.WriteLine("Close");
                }
            }
        }
    }
}
