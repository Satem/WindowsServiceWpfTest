namespace WpfApp.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Logic.Interfaces;
    using Models;
    using Prism.Mvvm;

    public class MainViewModel : BindableBase
    {
        private const int FetchServicesDelayInMilliseconds = 10000;

        private readonly IWindowsServiceHelper _windowsServiceHelper;
        private ObservableCollection<ServiceModel> _services;
        private ServiceModel _selectedService;


        public MainViewModel(
            IWindowsServiceHelper windowsServiceHelper
        )
        {
            _windowsServiceHelper = windowsServiceHelper;

            GetServiceListsWithDelay();
        }

        public ObservableCollection<ServiceModel> Services
        {
            get => _services;
            set
            {
                _services = value;
                RaisePropertyChanged(nameof(Services));
            }
        }

        public ServiceModel SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                RaisePropertyChanged(nameof(SelectedService));
            }
        }

        private async void GetServiceListsWithDelay()
        {
            while (true)
            {
                var services = await _windowsServiceHelper.GetWindowsServicesAsync();

                var selectedServiceName = _selectedService?.Name;
                Services = new ObservableCollection<ServiceModel>(services);
                ReselectSelectedService(selectedServiceName);

                await Task.Delay(FetchServicesDelayInMilliseconds);
            }
        }

        private void ReselectSelectedService(string selectedServiceName)
        {
            if (string.IsNullOrEmpty(selectedServiceName))
                return;

            SelectedService = Services.FirstOrDefault(service => service.Name == selectedServiceName);
        }
    }
}