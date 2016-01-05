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
        public int id;
        public string name;
        public string ipv4Address;
        public PhysicalAddress mac;
        //Interface statistics
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
            int i = 0;
            foreach (var interfaces in NetworkInterface.GetAllNetworkInterfaces())
            {

                NICS.Add(new NetworkInterfaces
                {
                    name = interfaces.Name,
                    id = NetworkInterfaces.getNextID(NICS),
                    mac = interfaces.GetPhysicalAddress(),
                    ipv4Address = GetAdapterIPAddress(interfaces)
                });
                i++;
            }
            return NICS;
        }

        public static string GetAdapterIPAddress(NetworkInterface ipInterface)
        {
            IPInterfaceProperties ipProperties = ipInterface.GetIPProperties();
            foreach (var ip in ipProperties.UnicastAddresses)
            {
                if ((ipInterface.OperationalStatus == OperationalStatus.Up) && (ip.Address.AddressFamily == AddressFamily.InterNetwork))
                {
                    return ip.Address.ToString();
                }
            }
            return "";
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
                    long sent = ConvertBytesToKbps(sendSpeed, lastCheckSend.TotalSeconds);
                    foreach (var update in NetworkInfo.Where(S => S.id == interfaceId))
                    {
                        update.bytesSent = interfaceStats.BytesSent;
                        update.byteSentLast = DateTime.Now;
                        if(update.bytesSentTotal != 0)
                        {
                            update.bytesSentTotal += (sendSpeed / 1024L) * 8L;
                        }
                        else
                        {
                            update.bytesSentTotal = 1;
                        }
                    }
                    return sent;
                case 1:
                    long recieveSpeed = interfaceStats.BytesReceived - interfaceStatistics.bytesReceived;
                    TimeSpan lastCheckRec = DateTime.Now - interfaceStatistics.byteReceivedLast;
                    long received = ConvertBytesToKbps(recieveSpeed, lastCheckRec.TotalSeconds);
                    foreach (var update in NetworkInfo.Where(S => S.id == interfaceId))
                    {
                        update.byteReceivedLast = DateTime.Now;
                        update.bytesReceived = interfaceStats.BytesReceived;
                        if(update.bytesReceivedTotal != 0)
                        {
                            update.bytesReceivedTotal += (recieveSpeed / 1024L) * 8L;
                        }
                        else
                        {
                            update.bytesReceivedTotal = 1;
                        }
                    }
                    return received;
                default:
                    return 0;
            }
        }
        public static long ConvertBytesToKbps(long speed, double seconds)
        {
            long ReturnSpeed = Convert.ToInt64(((double)(speed / 1024L) / seconds) * 8L);
            return ReturnSpeed;
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

