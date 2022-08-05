namespace WpfApp.Logic
{
    using System.Linq;
    using System.ServiceProcess;
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.Win32;
    using Models;

    public class WindowsServiceHelper : IWindowsServiceHelper
    {
        private readonly IAsyncTaskRunner _asyncTaskRunner;

        public WindowsServiceHelper(
            IAsyncTaskRunner asyncTaskRunner
            )
        {
            _asyncTaskRunner = asyncTaskRunner;
        }

        public Task<ServiceModel[]> GetWindowsServicesAsync()
        {
            return _asyncTaskRunner.RunFuncAsync(GetWindowsServices);
        }

        public Task StartServiceAsync(string serviceName)
        {
            return _asyncTaskRunner.RunActionAsync(() => StartService(serviceName));
        }

        public Task StopServiceAsync(string serviceName)
        {
            return _asyncTaskRunner.RunActionAsync(() => StopService(serviceName));
        }

        public Task PauseServiceAsync(string serviceName)
        {
            return _asyncTaskRunner.RunActionAsync(() => PauseService(serviceName));
        }

        public Task RestartServiceAsync(string serviceName)
        {
            return _asyncTaskRunner.RunActionAsync(() => RestartService(serviceName));
        }

        private ServiceModel[] GetWindowsServices()
        {
            var services = ServiceController.GetServices();

            var localMachineKey = Registry.LocalMachine;

            return services.Select(x => new ServiceModel()
            {
                Name = x.ServiceName,
                DisplayName = x.DisplayName,
                Status = x.Status,
                Account = GetAccountName(localMachineKey, x.ServiceName),
                CanPauseAndContinue = x.CanPauseAndContinue,
                CanStop = x.CanStop
            }).ToArray();
        }

        private string GetAccountName(RegistryKey localMachineKey, string serviceName)
        {
            var fileServiceKey = localMachineKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName);

            return (string)fileServiceKey?.GetValue("ObjectName") ?? string.Empty;
        }

        private void StartService(string serviceName)
        {
            var serviceController = new ServiceController(serviceName);
            serviceController.Start();
        }

        private void StopService(string serviceName)
        {
            var serviceController = new ServiceController(serviceName);
            serviceController.Stop();
        }

        private void PauseService(string serviceName)
        {
            var serviceController = new ServiceController(serviceName);
            serviceController.Pause();
        }

        private void RestartService(string serviceName)
        {
            var serviceController = new ServiceController(serviceName);
            serviceController.Pause();
        }
    }
}