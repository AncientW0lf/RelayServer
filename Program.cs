using System.IO;
using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using RelayServer.Server;
using System.Timers;

namespace RelayServer
{
    public class Program
    {
        public const string SettingsFile = "settings.json";

        public const string TemplateFile = "template.json";

        public static DomainLink[] Settings { get; private set; }

        private static readonly FileSystemWatcher _settingsReloader = new FileSystemWatcher
        {
            Filter = SettingsFile,
            Path = Environment.CurrentDirectory,
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.LastWrite
        };

        private static bool _reloadingSettings = false;

        public static int Main(string[] args)
        {
            if (args.Contains("-t") || args.Contains("--template"))
            {
                try
                {
                    CreateTemplateFile();
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"Could not create template file: {exc.Message}");
                    return 1;
                }
                Console.WriteLine(
                    $"Created file \"{TemplateFile}\". " +
                    "If you want to use this file for this application, " +
                    $"please rename it to \"{SettingsFile}\".");
                return 0;
            }

            _settingsReloader.Changed += (_, _) => ReloadSettings();
            _settingsReloader.EnableRaisingEvents = true;
            ReloadSettings();

            CreateHostBuilder(args).Build().Run();

            _settingsReloader.Dispose();

            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

#if DEBUG
                    webBuilder.UseUrls("http://*:8080");
#else
                    webBuilder.UseUrls("http://*:80", "https://*:443");
#endif
                });

        public static void CreateTemplateFile()
        {
            string value = JsonSerializer.Serialize<DomainLink[]>(new[]
            {
                new DomainLink(),
                new DomainLink()
            }, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(TemplateFile, value);
        }

        private static void ReloadSettings()
        {
            if (_reloadingSettings)
                return;

            _reloadingSettings = true;

            try
            {
                Settings = JsonSerializer.Deserialize<DomainLink[]>(File.ReadAllText(SettingsFile));
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Could not read settings at [{DateTime.Now:s}]: {exc.Message}");
                return;
            }

            Console.WriteLine($"Reloaded settings at [{DateTime.Now:s}].");

            _reloadingSettings = false;
        }
    }
}
