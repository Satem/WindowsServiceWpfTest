namespace WpfApp.Models
{
    using System.ServiceProcess;

    public class ServiceModel
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public ServiceControllerStatus Status { get; set; }
        public string Account { get; set; }
        public bool CanPauseAndContinue { get; set; }
        public bool CanStop { get; set; }
    }
}