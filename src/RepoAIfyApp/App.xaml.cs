using System.IO;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace RepoAIfyApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;

        public IServiceProvider ServiceProvider => _host.Services;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .UseSerilog((context, services, config) =>
                {
                    config
                        // Set the default minimum level to Debug to see more detailed messages.
                        .MinimumLevel.Debug()

                        // This filters out noisy messages from the .NET framework itself,
                        // keeping the debug output cleaner. You can adjust this as needed.
                        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)

                        // Add the new sink for the Visual Studio Debug window.
                        .WriteTo.Debug()

                        .WriteTo.File(
                            Path.Combine(
                                AppContext.BaseDirectory,
                                "Logs",
                                "RepoAIfyApp-.log"
                            ),
                            rollingInterval: RollingInterval.Day
                        )
                        .WriteTo.Sink(
                             // We need to get the ViewModelSink from the DI container
                             services.GetRequiredService<ViewModelSink>()
                        );
                })
                .Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register the interface and its concrete implementation
            services.AddSingleton<IDialogService, WpfDialogService>();
            services.AddSingleton<UILogRelayService>();
            services.AddSingleton<ViewModelSink>();
            // Now you can register the ViewModel again!
            services.AddSingleton<MainWindowViewModel>();
            // Register your services
            services.AddSingleton<MainWindow>();
            services.AddTransient<RepoAIfyLib.Services.TreeViewDataService>();
            services.AddTransient<RepoAIfyLib.Services.OptionsLoader>();
            services.AddTransient<RepoAIfyLib.Services.FileProcessor>();
            services.AddTransient<RepoAIfyLib.ConverterRunner>();
            services.AddTransient<Func<int, RepoAIfyLib.Services.MarkdownGenerator>>(provider =>
                (maxChunkSizeKb) => new RepoAIfyLib.Services.MarkdownGenerator(
                    provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RepoAIfyLib.Services.MarkdownGenerator>>(),
                    maxChunkSizeKb
                ));
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }
            base.OnExit(e);
        }
    }
}