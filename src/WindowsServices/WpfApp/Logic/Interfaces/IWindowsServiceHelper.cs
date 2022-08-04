namespace WpfApp.Logic.Interfaces
{
    using System.Threading.Tasks;
    using Models;

    public interface IWindowsServiceHelper
    {
        Task<ServiceModel[]> GetWindowsServicesAsync();
    }
}