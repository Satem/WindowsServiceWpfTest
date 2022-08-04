namespace WpfApp.Converters
{
    using System;
    using System.Globalization;
    using System.ServiceProcess;
    using System.Windows;
    using System.Windows.Data;

    [ValueConversion(typeof(ServiceControllerStatus), typeof(string))]

    public class ServiceControllerStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            var status = (ServiceControllerStatus)value;
            switch (status)
            {
                case ServiceControllerStatus.StartPending:
                    return "Pending Start";
                case ServiceControllerStatus.ContinuePending:
                    return "Pending Continue";
                case ServiceControllerStatus.Running:
                    return "Running";
                case ServiceControllerStatus.PausePending:
                    return "Pending Pause";
                case ServiceControllerStatus.Paused:
                    return "Paused";
                case ServiceControllerStatus.StopPending:
                    return "Pending Stop";
                case ServiceControllerStatus.Stopped:
                    return "Stopped";
                default: return "Unknown";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringValue = value as string;
            if (string.IsNullOrEmpty(stringValue))
                return DependencyProperty.UnsetValue;

            return
                Enum.TryParse<ServiceControllerStatus>(stringValue, out var resultStatus)
                    ? resultStatus
                    : DependencyProperty.UnsetValue;
        }
    }
}