namespace WpfApp.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using Logic.Interfaces;
    using Mappers;
    using Models;
    using Prism.Commands;
    using Prism.Mvvm;
    using ViewModelHelpers;

    public class MainViewModel : BindableBase
    {
        private const int FetchServicesDelayInMilliseconds = 10000;

        private readonly Dictionary<string, ServiceViewModel> _serviceNameToServiceDictionary;
        private readonly ServiceViewModelMapper _serviceViewModelMapper;
        private readonly ServiceStatusChangeTaskStore _serviceStatusChangeTaskStore;

        private readonly IWindowsServiceHelper _windowsServiceHelper;
        private readonly IWindowsPrincipleChecker _windowsPrincipleChecker;
        private ServiceViewModel _selectedService;


        public MainViewModel(
            IWindowsServiceHelper windowsServiceHelper,
            IWindowsPrincipleChecker windowsPrincipleChecker,
            ServiceViewModelMapper serviceViewModelMapper,
            ServiceStatusChangeTaskStore serviceStatusChangeTaskStore
        )
        {
            _windowsServiceHelper = windowsServiceHelper;
            _windowsPrincipleChecker = windowsPrincipleChecker;
            _serviceViewModelMapper = serviceViewModelMapper;
            _serviceStatusChangeTaskStore = serviceStatusChangeTaskStore;

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
                UnsubscribeOnServicePropertiesChange(value);
                _selectedService = value;
                RaisePropertyChanged(nameof(SelectedService));
                SubscribeOnServicePropertiesChange(_selectedService);
                NotifyViewOfPossibleCommandAvailabilityChange();
            }
        }

        public ICommand StartCommand { get; }

        public ICommand StopCommand { get; }

        public ICommand PauseCommand { get; }

        public ICommand RestartCommand { get; }

        public bool CanStart => _selectedService != null
                                && _serviceStatusChangeTaskStore.IsThereAnyRunningTask(_selectedService.Name) == false
                                && _selectedService.CanNowBeStarted;

        public bool CanStop => _selectedService != null
                               && _serviceStatusChangeTaskStore.IsThereAnyRunningTask(_selectedService.Name) == false
                               && _selectedService.CanNowBeStopped;

        public bool CanPause => _selectedService != null
                                && _serviceStatusChangeTaskStore.IsThereAnyRunningTask(_selectedService.Name) == false
                                && _selectedService.CanNowBePaused;

        public bool CanRestart => _selectedService != null
                                  && _serviceStatusChangeTaskStore.IsThereAnyRunningTask(_selectedService.Name) == false
                                  && _selectedService.CanNowBePaused;

        private async void Start()
        {
            await RunStatusChangeTaskAsync(_windowsServiceHelper.StartServiceAsync);
        }

        private async void Stop()
        {
            await RunStatusChangeTaskAsync(_windowsServiceHelper.StopServiceAsync);
        }

        private async void Pause()
        {
            await RunStatusChangeTaskAsync(_windowsServiceHelper.StopServiceAsync);
        }

        private async void Restart()
        {
            await RunStatusChangeTaskAsync(_windowsServiceHelper.RestartServiceAsync);
        }

        private async Task RunStatusChangeTaskAsync(
            Func<string, CancellationToken, Task> statusChangeMethod)
        {
            if (_selectedService == null)
                return;

            if (_windowsPrincipleChecker.IsRunUnderAdministrator == false)
            {
                var message = "In order to start or stop services, please run the programme as administrator";
                MessageBox.Show(message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var selectedServiceName = _selectedService.Name;
            try
            {

                var statusChangeTask = _serviceStatusChangeTaskStore.StartAndStoreNewTask(selectedServiceName, statusChangeMethod);
            
                await UpdateServiceStatus(selectedServiceName);

                await statusChangeTask;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Unable to change status", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _serviceStatusChangeTaskStore.StopAndRemoveTaskIfExists(selectedServiceName);
            await UpdateServiceStatus(selectedServiceName);
        }

        private async Task UpdateServiceStatus(string serviceName)
        {
            var latestModel = await _windowsServiceHelper.GetServiceByNameAsync(serviceName);
            if (_serviceNameToServiceDictionary.ContainsKey(serviceName) == false)
                return;

            var serviceViewModel = _serviceNameToServiceDictionary[serviceName];
            _serviceViewModelMapper.UpdateViewModel(serviceViewModel, latestModel);
        }

        #region ServicePropertiesChangeSubscription

        
        private void SubscribeOnServicePropertiesChange(ServiceViewModel service)
        {
            if (service == null)
                return;

            service.PropertyChanged += ServicePropertiesChange;
        }

        private void UnsubscribeOnServicePropertiesChange(ServiceViewModel service)
        {
            if (service == null)
                return;

            service.PropertyChanged -= ServicePropertiesChange;
        }

        private void ServicePropertiesChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ServiceViewModel.Status)
                || e.PropertyName == nameof(ServiceViewModel.CanStop)
                || e.PropertyName == nameof(ServiceViewModel.CanPauseAndContinue))
                NotifyViewOfPossibleCommandAvailabilityChange();
        }

        #endregion

        private void NotifyViewOfPossibleCommandAvailabilityChange()
        {
            RaisePropertyChanged(nameof(CanStart));
            RaisePropertyChanged(nameof(CanStop));
            RaisePropertyChanged(nameof(CanPause));
            RaisePropertyChanged(nameof(CanRestart));
        }

        #region LoadingServices

        
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
                _serviceStatusChangeTaskStore.StopAndRemoveTaskIfExists(service.Name);
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
            foreach (var service in newServices)
                _serviceNameToServiceDictionary[service.Name] = service;

            Services.AddRange(newServices);
        }

        #endregion
    }
}