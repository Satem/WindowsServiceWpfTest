namespace WpfApp.Logic
{
    using System;
    using System.Linq;
    using System.ServiceProcess;
    using System.Threading;
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.Win32;
    using Models;

    public class WindowsServiceHelper : IWindowsServiceHelper
    {
        private const int IntervalBetweenCancellationChecksInMilliseconds = 1000;
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

        public Task<ServiceModel> GetServiceByNameAsync(string serviceName)
        {
            return _asyncTaskRunner.RunFuncAsync(() => GetServiceByName(serviceName));
        }

        public Task StartServiceAsync(string serviceName, CancellationToken cancellationToken)
        {
            return _asyncTaskRunner.RunActionAsync(() => StartService(serviceName, cancellationToken));
        }

        public Task StopServiceAsync(string serviceName, CancellationToken cancellationToken)
        {
            return _asyncTaskRunner.RunActionAsync(() => StopService(serviceName, cancellationToken));
        }

        public Task PauseServiceAsync(string serviceName, CancellationToken cancellationToken)
        {
            return _asyncTaskRunner.RunActionAsync(() => PauseService(serviceName, cancellationToken));
        }

        public Task RestartServiceAsync(string serviceName, CancellationToken cancellationToken)
        {
            return _asyncTaskRunner.RunActionAsync(() => RestartService(serviceName, cancellationToken));
        }

        private ServiceModel[] GetWindowsServices()
        {
            var services = ServiceController.GetServices();

            var localMachineKey = Registry.LocalMachine;

            return services.Select(service => MapModelFromService(service, localMachineKey)).ToArray();
        }

        private ServiceModel GetServiceByName(string serviceName)
        {
            var localMachineKey = Registry.LocalMachine;
            var service = new ServiceController(serviceName);
            return MapModelFromService(service, localMachineKey);
        }

        private ServiceModel MapModelFromService(ServiceController service, RegistryKey registryKey)
        {
            return new ServiceModel
            {
                Name = service.ServiceName,
                DisplayName = service.DisplayName,
                Status = service.Status,
                Account = GetAccountName(registryKey, service.ServiceName),
                CanPauseAndContinue = service.CanPauseAndContinue,
                CanStop = service.CanStop
            };
        }

        private string GetAccountName(RegistryKey localMachineKey, string serviceName)
        {
            var fileServiceKey = localMachineKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName);

            return (string)fileServiceKey?.GetValue("ObjectName") ?? string.Empty;
        }

        private void StartService(string serviceName, CancellationToken cancellationToken)
        {
            var serviceController = new ServiceController(serviceName);
            if (serviceController.Status == ServiceControllerStatus.Paused)
                RunStatusChangeCommand(
                    serviceController,
                    serviceController.Continue,
                    cancellationToken);
            else
                RunStatusChangeCommand(
                    serviceController,
                    serviceController.Start,
                    cancellationToken);
        }

        private void StopService(string serviceName, CancellationToken cancellationToken)
        {
            var serviceController = new ServiceController(serviceName);
            RunStatusChangeCommand(
                serviceController,
                serviceController.Stop,
                cancellationToken);
        }

        private void PauseService(string serviceName, CancellationToken cancellationToken)
        {
            var serviceController = new ServiceController(serviceName);
            RunStatusChangeCommand(
                serviceController,
                serviceController.Pause,
                cancellationToken);
        }

        private void RestartService(string serviceName, CancellationToken cancellationToken)
        {
            var serviceController = new ServiceController(serviceName);
            RunStatusChangeCommand(
                serviceController,
                serviceController.Stop,
                cancellationToken);

            if (serviceController.Status == ServiceControllerStatus.Paused)
                RunStatusChangeCommand(
                    serviceController,
                    serviceController.Start,
                    cancellationToken);
        }

        private void RunStatusChangeCommand(
            ServiceController service,
            Action command,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            command();
            do
            {
                Thread.Sleep(IntervalBetweenCancellationChecksInMilliseconds);
            } while (IsPendingStatus(service.Status) &&
                     cancellationToken.IsCancellationRequested == false);
        }

        private bool IsPendingStatus(ServiceControllerStatus serviceStatus)
        {
            return serviceStatus == ServiceControllerStatus.PausePending
                   || serviceStatus == ServiceControllerStatus.ContinuePending
                   || serviceStatus == ServiceControllerStatus.StartPending
                   || serviceStatus == ServiceControllerStatus.StopPending;
        }
    }
}