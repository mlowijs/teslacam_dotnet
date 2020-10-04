using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TeslaCam.Contracts;

namespace TeslaCam.Services
{
    public class KernelService : IKernelService
    {
        private const string MassStorageGadgetModuleName = "g_mass_storage";

        private readonly ILogger<KernelService> _logger;

        
        public KernelService(ILogger<KernelService> logger)
        {
            _logger = logger;
        }
        
        public void RemoveMassStorageGadgetModule()
        {
            _logger.LogDebug("Removing mass storage gadget module");
            
            Process.Start("/usr/bin/rmmod", MassStorageGadgetModuleName).WaitForExit();
        }

        public void LoadMassStorageGadgetModule()
        {
            _logger.LogDebug("Probing mass storage gadget module");

            Process.Start("/usr/bin/modprobe", MassStorageGadgetModuleName).WaitForExit();
        }
    }
}