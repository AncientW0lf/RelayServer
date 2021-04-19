using System.IO;
using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using RelayServer.Server;

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

#pragma warning disable CS0414
        private static bool _noHttps = false;
#pragma warning restore CS0414

        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, _) => DisposeVars();

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

            ToggleHttps(args);

            _settingsReloader.Changed += (_, _) => ReloadSettings();
            _settingsReloader.EnableRaisingEvents = true;
            ReloadSettings();

            CreateHostBuilder(args).Build().Run();

            DisposeVars();

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
                    if (_noHttps)
                        webBuilder.UseUrls("http://*:80");
                    else
                    {
                        webBuilder.ConfigureKestrel(o =>
                        {
                            o.ConfigureHttpsDefaults(o2 =>
                            {
                                o2.ServerCertificate = X509Certificate2.CreateFromPemFile("cert/fullchain.pem", "cert/privkey.pem");
                            });
                        });

                        webBuilder.UseUrls("http://*:80", "https://*:443");
                    }
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

        private static void ToggleHttps(string[] args)
        {
            if (!args.Contains("-h") && !args.Contains("--no-https"))
                return;

            _noHttps = true;
        }

        private static void DisposeVars()
        {
            _settingsReloader.Dispose();
        }
    }
}
