namespace WpfApp
{
    using System.Windows;
    using Logic;
    using Logic.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            _serviceProvider = CreateServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }

        private ServiceProvider CreateServiceProvider()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient<MainWindow>();
            serviceCollection.AddTransient<MainViewModel>();

            serviceCollection.AddTransient<IWindowsServiceHelper, WindowsServiceHelper>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}