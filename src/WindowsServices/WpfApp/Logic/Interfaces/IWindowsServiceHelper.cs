namespace WpfApp.Logic.Interfaces
{
    using System.ServiceProcess;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    public interface IWindowsServiceHelper
    {
        Task<ServiceModel[]> GetWindowsServicesAsync();
        Task<ServiceModel> GetServiceByNameAsync(string serviceName);
        Task StartServiceAsync(string serviceName, CancellationToken cancellationToken);
        Task StopServiceAsync(string serviceName, CancellationToken cancellationToken);
        Task PauseServiceAsync(string serviceName, CancellationToken cancellationToken);
        Task RestartServiceAsync(string serviceName, CancellationToken cancellationToken);
    }
}