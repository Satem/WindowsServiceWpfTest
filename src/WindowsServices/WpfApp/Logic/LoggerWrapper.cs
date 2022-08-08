namespace WpfApp.Logic
{
    using System;
    using Interfaces;
    using Serilog;

    public class LoggerWrapper : ILoggerWrapper
    {
        public void LogError<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            Log.Error(exception, messageTemplate, propertyValue);
        }

        public void LogError<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Log.Error(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        public void LogFatal(Exception exception, string messageTemplate)
        {
            Log.Fatal(exception, messageTemplate);

        }
    }
}