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
        public Task<ServiceModel[]> GetWindowsServicesAsync()
        {
            var task = new Task<ServiceModel[]>(GetWindowsServices);
            task.Start();
            return task;
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
                Account = GetAccountName(localMachineKey, x.ServiceName)
            }).ToArray();
        }

        private string GetAccountName(RegistryKey localMachineKey, string serviceName)
        {
            var fileServiceKey = localMachineKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName);

            return (string)fileServiceKey?.GetValue("ObjectName") ?? string.Empty;
        }
    }
}