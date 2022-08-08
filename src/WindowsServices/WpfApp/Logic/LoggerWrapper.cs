namespace WpfApp.Logic
{
    using System;
    using Interfaces;
    using Serilog;

    public class LoggerWrapper : ILoggerWrapper
    {
        public void LogInformation(string message)
        {
            Log.Information(message);
        }

        public void LogError(Exception exception, string messageTemplate)
        {
            Log.Error(exception, messageTemplate);
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