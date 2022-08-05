namespace WpfApp.ViewModels
{
    using System.ServiceProcess;
    using Prism.Mvvm;

    public class ServiceViewModel : BindableBase
    {
        private string _displayName;
        private ServiceControllerStatus _status;
        private string _account;
        private bool _canPauseAndContinue;
        private bool _canStop;

        public ServiceViewModel(string serviceName)
        {
            Name = serviceName;
        }

        public string Name { get; }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName == value)
                    return;
                _displayName = value;
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        public ServiceControllerStatus Status
        {
            get => _status;
            set
            {
                if (_status == value)
                    return;
                _status = value;
                RaisePropertyChanged(nameof(Status));
            }
        }

        public string Account
        {
            get => _account;
            set
            {
                if (_account == value)
                    return;
                _account = value;
                RaisePropertyChanged(nameof(Account));
            }
        }

        public bool CanPauseAndContinue
        {
            get => _canPauseAndContinue;
            set
            {
                if (_canPauseAndContinue == value)
                    return;
                _canPauseAndContinue = value;
                RaisePropertyChanged(nameof(CanPauseAndContinue));
            }
        }

        public bool CanStop
        {
            get => _canStop;
            set
            {
                if (_canStop == value)
                    return;
                _canStop = value;
                RaisePropertyChanged(nameof(CanStop));
            }
        }

        public bool CanNowBeStarted => Status == ServiceControllerStatus.Stopped || Status == ServiceControllerStatus.Paused;
        public bool CanNowBeStopped => CanStop && Status == ServiceControllerStatus.Running;
        public bool CanNowBePaused => CanPauseAndContinue && Status == ServiceControllerStatus.Running;
    }
}