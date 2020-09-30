using System.Diagnostics;
using TeslaCam.Contracts;

namespace TeslaCam.Services
{
    public class KernelService : IKernelService
    {
        private const string MassStorageGadgetModuleName = "g_mass_storage";
        
        public void RemoveMassStorageGadgetModule()
        {
            Process.Start("/usr/bin/rmmod", MassStorageGadgetModuleName).WaitForExit();
        }

        public void LoadMassStorageGadgetModule()
        {
            Process.Start("/usr/bin/modprobe", MassStorageGadgetModuleName).WaitForExit();
        }
    }
}