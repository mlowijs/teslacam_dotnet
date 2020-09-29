using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TeslaCam.Contracts;

namespace TeslaCam.Services
{
    public class NetworkService : INetworkService
    {
        private const int PingAttempts = 3;
        private const int PingTimeout = 1000;
        
        private static readonly IPAddress[] IpAddresses =
        {
            // Cloudflare
            IPAddress.Parse("1.1.1.1"), 
            IPAddress.Parse("1.0.0.1"),
            // Google
            IPAddress.Parse("8.8.8.8"), 
            IPAddress.Parse("8.8.4.4"),
            // OpenDNS
            IPAddress.Parse("208.67.222.222"),
            IPAddress.Parse("208.67.220.220"),
        };

        private readonly Random _random;

        public NetworkService()
        {
            _random = new Random();
        }
        
        public async Task<bool> IsConnectedToInternet()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;
            
            var ping = new Ping();
            
            for (var i = 0; i < PingAttempts; i++)
            {
                var ipAddress = IpAddresses[_random.Next(0, IpAddresses.Length)];

                var reply = await ping.SendPingAsync(ipAddress, PingTimeout);

                if (reply.Status == IPStatus.Success)
                    return true;
            }

            return false;
        }
    }
}