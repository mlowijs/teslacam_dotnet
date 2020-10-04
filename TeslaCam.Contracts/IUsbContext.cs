using System;

namespace TeslaCam.Contracts
{
    public interface IUsbContext : IDisposable
    {
        void Mount(bool readWrite);
    }
}