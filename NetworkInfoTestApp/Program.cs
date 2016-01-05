using NetworkInterfaceInfo;
using System.Timers;
using System;
namespace NetworkInfoTestApp
{
    class Program
    {
        public static Timer FastTimer = new Timer(1000);
        static void Main(string[] args)
        {
            FastTimer.Elapsed += new ElapsedEventHandler(Timer_elapsed);
            FastTimer.Enabled = true;
            Console.ReadLine();
        }
        static void Timer_elapsed(object sender, ElapsedEventArgs e)
        {
            long uploadSpeed = NetworkInterfaceSpeed.GetNetworkTraffic(0);
            long downloadSpeed = NetworkInterfaceSpeed.GetNetworkTraffic(1);
            long totalUploads = NetworkInterfaceSpeed.GetTotalNetworkTraffic(0);
            long totalDownloads = NetworkInterfaceSpeed.GetTotalNetworkTraffic(1);
            Console.Clear();
            Console.WriteLine(string.Format("U {0} : D{1}", uploadSpeed, downloadSpeed));
            Console.WriteLine(string.Format("U {0} : D{1}", totalUploads, totalDownloads));
        }

    }
}
