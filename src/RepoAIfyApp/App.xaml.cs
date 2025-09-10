using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Windows;

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
                .UseSerilog((context, config) =>
                {
                    config.WriteTo.File(
                        Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "RepoAIfy",
                            "RepoAIfy-.log"
                        ),
                        rollingInterval: RollingInterval.Day
                    );
                })
                .Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register your services
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<RepoAIfyLib.Services.TreeViewDataService>();
            services.AddTransient<RepoAIfyLib.Services.OptionsLoader>();
            services.AddTransient<RepoAIfyLib.Services.FileProcessor>();
            services.AddTransient<RepoAIfyLib.Services.MarkdownGenerator>();
            services.AddTransient<RepoAIfyLib.ConverterRunner>();
            // ... register other services from RepoAIfyLib
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