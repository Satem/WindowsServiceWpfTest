namespace WpfApp.Logic.Interfaces
{
    using System;

    public interface ILoggerWrapper
    {
        void LogInformation(string message);
        void LogError(Exception exception, string messageTemplate);
        void LogError<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        void LogFatal(Exception exception, string messageTemplate);
    }
}