using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace ipset

{
    public static class NetworkHelper
    {
        public static void SetIPAddress(string interfaceName, string ipAddress)
        {
            try
            {
                NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces()
                                                    .FirstOrDefault(ni => ni.Name == interfaceName);

                if (networkInterface != null)
                {
                    // Your logic to set the IP address goes here
                    Console.WriteLine($"Setting IP address for {interfaceName} to {ipAddress}");
                }
                else
                {
                    Console.WriteLine($"Network interface {interfaceName} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting IP address: {ex.Message}");
            }
        }
    }
}