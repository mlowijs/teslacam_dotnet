using System;

namespace TeslaCam.Contracts
{
    public interface IUsbFileSystemContext : IDisposable
    {
        void Mount(bool readWrite);
        void Unmount();
    }
}