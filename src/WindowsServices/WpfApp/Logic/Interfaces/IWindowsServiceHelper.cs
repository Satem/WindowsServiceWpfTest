namespace WpfApp.Logic.Interfaces
{
    using System.Threading.Tasks;
    using Models;

    public interface IWindowsServiceHelper
    {
        Task<ServiceModel[]> GetWindowsServicesAsync();
        Task StartServiceAsync(string serviceName);
        Task StopServiceAsync(string serviceName);
        Task PauseServiceAsync(string serviceName);
        Task RestartServiceAsync(string serviceName);
    }
}