namespace TeslaCam.Contracts
{
    public interface IUsbService
    {
        IUsbContext AcquireUsbContext();
    }
}