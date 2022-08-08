namespace WpfApp.Logic.Interfaces
{
    using System;

    public interface ILoggerWrapper
    {
        void LogError<T>(Exception exception, string messageTemplate, T propertyValue);
        void LogError<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        void LogFatal(Exception exception, string messageTemplate);
    }
}