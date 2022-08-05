﻿namespace WpfApp.Logic
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

        public Task<ServiceControllerStatus> GetServiceStatusAsync(string serviceName)
        {
            return _asyncTaskRunner.RunFuncAsync(() => GetServiceStatus(serviceName));
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

            return services.Select(x => new ServiceModel
            {
                Name = x.ServiceName,
                DisplayName = x.DisplayName,
                Status = x.Status,
                Account = GetAccountName(localMachineKey, x.ServiceName),
                CanPauseAndContinue = x.CanPauseAndContinue,
                CanStop = x.CanStop
            }).ToArray();
        }

        private ServiceControllerStatus GetServiceStatus(string serviceName)
        {
            return new ServiceController(serviceName).Status;
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
                serviceController.Pause,
                cancellationToken);

            if (serviceController.Status == ServiceControllerStatus.Paused)
                RunStatusChangeCommand(
                    serviceController,
                    serviceController.Continue,
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