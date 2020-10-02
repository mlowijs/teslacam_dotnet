using System;

namespace TeslaCam.Contracts
{
    public interface IUsbService
    {
        void ExecuteWithMountedFileSystem(Action codeToExecute, bool readWrite = false);
    }
}