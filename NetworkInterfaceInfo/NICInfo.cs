using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Linq;
namespace NetworkInterfaceInfo
{
    public class NetworkInterfaces
    {
        public string name { get; set; }
        public int id { get; set; }
        public long bytesSent;
        public long bytesReceived;
        public long bytesSentTotal;
        public long bytesReceivedTotal;
        public DateTime byteSentLast;
        public DateTime byteReceivedLast;

        public static int getNextID(List<NetworkInterfaces> NICs)
        {
            int highestId = -1;
            foreach (var id in NICs)
            {
                if (id.id > highestId)
                {
                    highestId = id.id;
                }
            }
            return highestId + 1;
        }
    }
    public class NetworkInterfaceSpeed
    {
        public static List<NetworkInterfaces> NetworkInfo = new List<NetworkInterfaces>();

        public static List<NetworkInterfaces> GetAllNetworkInterfaceInfo()
        {
            List<NetworkInterfaces> NICS = new List<NetworkInterfaces>();

            foreach (var interfaces in NetworkInterface.GetAllNetworkInterfaces())
            {
                NICS.Add(new NetworkInterfaces
                {
                    name = interfaces.Name,
                    id = NetworkInterfaces.getNextID(NICS)
                });
            }
            return NICS;
        }

        /// <summary>
        /// Get the network traffic for a specific interface.
        /// </summary>
        /// <param name="type">Upload = 0, download = 1</param>
        /// <param name="interfaceId">The ID of the interface you wish to query</param>
        /// <returns></returns>
        public static long GetNetworkTraffic(int type, int interfaceId = 0)
        {
            if(NetworkInfo.Count == 0)
            {
                NetworkInfo = GetAllNetworkInterfaceInfo();
            }
            var interfaceStatistics = NetworkInfo.Single(s => s.id == interfaceId);
            IPv4InterfaceStatistics interfaceStats = NetworkInterface.GetAllNetworkInterfaces()[interfaceId].GetIPv4Statistics();
            switch (type)
            {
                case 0:
                    long sendSpeed = interfaceStats.BytesSent - interfaceStatistics.bytesSent;
                    TimeSpan lastCheckSend = DateTime.Now - interfaceStatistics.byteSentLast;
                    long sent = Convert.ToInt64(((double)(sendSpeed / 1024L) / lastCheckSend.TotalSeconds) * 8L);
                    foreach (var update in NetworkInfo.Where(S => S.id == interfaceId))
                    {
                        update.bytesSent = interfaceStats.BytesSent;
                        update.byteSentLast = DateTime.Now;
                        update.bytesSentTotal += sent;
                    }
                    return sent;
                case 1:
                    long recieveSpeed = interfaceStats.BytesReceived - interfaceStatistics.bytesReceived;
                    TimeSpan lastCheckRec = DateTime.Now - interfaceStatistics.byteReceivedLast;
                    long received = Convert.ToInt64(((double)(recieveSpeed / 1024L) / lastCheckRec.TotalSeconds) * 8L);
                    foreach (var update in NetworkInfo.Where(S => S.id == interfaceId))
                    {
                        update.byteReceivedLast = DateTime.Now;
                        update.bytesReceived = interfaceStats.BytesReceived;
                        update.bytesReceivedTotal += received;
                    }
                    return received;
                default:
                    return 0;
            }
        }
        public static long GetTotalNetworkTraffic(int type, int id = 0)
        {
            long speed = 0;
            var totals = NetworkInfo.Single(s => s.id == id);
            switch (type)
            {
                case 0:
                    speed = totals.bytesSentTotal;
                    break;
                case 1:
                    speed = totals.bytesReceivedTotal;
                    break;
            }
            return speed;
        }
        /// <summary>
        /// Gets local IP Address
        /// </summary>
        /// <returns>Local IP Address as string</returns>
        public static string GetLocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}

