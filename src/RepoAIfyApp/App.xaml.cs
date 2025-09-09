using System.Configuration;
using System.Data;
using System.Windows;

using Serilog;

namespace RepoAIfyApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("RepoAIfyApp.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }

}
