namespace WpfApp.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Logic.Interfaces;
    using Mappers;
    using Models;
    using Prism.Commands;
    using Prism.Mvvm;

    public class MainViewModel : BindableBase
    {
        private const int FetchServicesDelayInMilliseconds = 10000;

        private readonly Dictionary<string, ServiceViewModel> _serviceNameToServiceDictionary;
        private readonly ServiceViewModelMapper _serviceViewModelMapper;

        private readonly IWindowsServiceHelper _windowsServiceHelper;
        private ServiceViewModel _selectedService;


        public MainViewModel(
            IWindowsServiceHelper windowsServiceHelper,
            ServiceViewModelMapper serviceViewModelMapper
        )
        {
            _windowsServiceHelper = windowsServiceHelper;
            _serviceViewModelMapper = serviceViewModelMapper;

            StartCommand = new DelegateCommand(Start);
            StopCommand = new DelegateCommand(Stop);
            PauseCommand = new DelegateCommand(Pause);
            RestartCommand = new DelegateCommand(Restart);

            _serviceNameToServiceDictionary = new Dictionary<string, ServiceViewModel>();
            Services = new ObservableCollection<ServiceViewModel>();

            GetServiceListsWithDelay();
        }

        public ObservableCollection<ServiceViewModel> Services { get; }

        public ServiceViewModel SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                RaisePropertyChanged(nameof(SelectedService));
            }
        }

        public ICommand StartCommand { get; }

        public ICommand StopCommand { get; }

        public ICommand PauseCommand { get; }

        public ICommand RestartCommand { get; }

        private async void Start()
        {
            if (_selectedService != null)
                await _windowsServiceHelper.StartServiceAsync(_selectedService.Name);
        }

        private async void Stop()
        {
            if (_selectedService != null)
                await _windowsServiceHelper.StopServiceAsync(_selectedService.Name);
        }

        private async void Pause()
        {
            if (_selectedService != null)
                await _windowsServiceHelper.PauseServiceAsync(_selectedService.Name);
        }

        private async void Restart()
        {
            if (_selectedService != null)
                await _windowsServiceHelper.RestartServiceAsync(_selectedService.Name);
        }

        private async void GetServiceListsWithDelay()
        {
            while (true)
            {
                var services = await _windowsServiceHelper.GetWindowsServicesAsync();

                RefreshServices(services);

                await Task.Delay(FetchServicesDelayInMilliseconds);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void RefreshServices(IReadOnlyCollection<ServiceModel> services)
        {
            RemoveFromDisplayDeletedServices(services);

            var newServicesToDisplay = new List<ServiceViewModel>();
            foreach (var service in services)
                if (ServiceIsAlreadyDisplayed(service.Name))
                {
                    var displayedService = GetDisplayedService(service.Name);
                    _serviceViewModelMapper.UpdateViewModel(displayedService, service);
                }
                else
                {
                    var serviceToDisplay = _serviceViewModelMapper.MapFrom(service);
                    newServicesToDisplay.Add(serviceToDisplay);
                }

            if (newServicesToDisplay.Any() == false)
                return;

            DisplayNewService(newServicesToDisplay);
        }

        private void RemoveFromDisplayDeletedServices(IReadOnlyCollection<ServiceModel> services)
        {
            var newServicesNameHashSet = services.Select(x => x.Name).ToHashSet();
            var servicesToRemove =
                Services
                    .Where(displayedService => newServicesNameHashSet.Contains(displayedService.Name) == false)
                    .ToArray();
            foreach (var service in servicesToRemove)
            {
                _serviceNameToServiceDictionary.Remove(service.Name);
                Services.Remove(service);
            }
        }

        private bool ServiceIsAlreadyDisplayed(string serviceName)
        {
            return _serviceNameToServiceDictionary.ContainsKey(serviceName);
        }

        private ServiceViewModel GetDisplayedService(string serviceName)
        {
            return _serviceNameToServiceDictionary[serviceName];
        }

        private void DisplayNewService(IReadOnlyCollection<ServiceViewModel> newServices)
        {
            foreach (var service in newServices) _serviceNameToServiceDictionary[service.Name] = service;

            Services.AddRange(newServices);
        }
    }
}