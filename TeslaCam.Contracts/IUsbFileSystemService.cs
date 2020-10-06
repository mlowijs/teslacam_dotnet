namespace TeslaCam.Contracts
{
    public interface IUsbFileSystemService
    {
        IUsbFileSystemContext AcquireContext();
    }
}