namespace WpfApp
{
    using System.Windows;
    using System.Windows.Threading;
    using Logic;
    using Logic.Interfaces;
    using Mappers;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using ViewModelHelpers;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureLogging(serviceCollection);
            ConfigureServiceCollection(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureLogging(ServiceCollection serviceCollection)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\windows_services_app.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }

        private void ConfigureServiceCollection(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<MainWindow>();
            serviceCollection.AddTransient<MainViewModel>();
            serviceCollection.AddTransient<ServiceViewModelMapper, ServiceViewModelMapper>();
            serviceCollection.AddTransient<ServiceStatusChangeTaskStore, ServiceStatusChangeTaskStore>();

            serviceCollection.AddTransient<IWindowsServiceHelper, WindowsServiceHelper>();
            serviceCollection.AddTransient<IAsyncTaskRunner, AsyncTaskRunner>();
            serviceCollection.AddTransient<IWindowsPrincipleChecker, WindowsPrincipleChecker>();
            serviceCollection.AddTransient<ILoggerWrapper, LoggerWrapper>();
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Fatal error has occurred and the application must be closed. Please refer to a log file for more detailed information" , "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
            var logger = _serviceProvider.GetRequiredService<ILoggerWrapper>();
            logger.LogFatal(e.Exception, "Unhandled exception");
        }
    }
}