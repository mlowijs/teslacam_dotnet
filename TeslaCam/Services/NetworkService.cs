using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

        private readonly ILogger<NetworkService> _logger;
        
        private readonly Random _random;

        public NetworkService(ILogger<NetworkService> logger)
        {
            _logger = logger;
            
            _random = new Random();
        }
        
        public async Task<bool> IsConnectedToInternetAsync()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;
            
            var ping = new Ping();
            
            for (var i = 0; i < PingAttempts; i++)
            {
                var ipAddress = IpAddresses[_random.Next(0, IpAddresses.Length)];

                try
                {
                    var reply = await ping.SendPingAsync(ipAddress, PingTimeout);

                    if (reply.Status == IPStatus.Success)
                        return true;
                }
                catch (Exception ex) when (ex is not PingException)
                {
                    _logger.LogError(ex, "Error occured while sending ping:");
                }
            }

            return false;
        }
    }
}