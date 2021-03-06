﻿/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;
using System.ComponentModel.DataAnnotations;
using FeedToMastodon.Lib;
using FeedToMastodon.Lib.Interfaces;
using FeedToMastodon.Lib.Models.Configuration;
using Serilog;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FeedToMastodon.Cli
{
    // Default/Main Command.
    // Is executed, when no options or commands are given
    //
    // Also contains some static default variables
    // and the registration of dependency injection
    [Command(
        Name = Constants.PROGRAMNAME,
        FullName = Constants.PROGRAMFULLNAME,
        Description = "Pushes datafeeds to mastodon.",
        ExtendedHelpText = Constants.EXTENDEDHELPTEXT
    )]
    [Subcommand(typeof(Commands.RegisterApplication))]
    [Subcommand(typeof(Commands.Run))]
    [Subcommand(typeof(Commands.Toot))]
    [VersionOption(Constants.VERSION)]
    [HelpOption]
    class Program
    {
        private readonly ILogger<Program> log;
        private readonly IAppConfiguration cfg;

        public static bool Debug {
            get
            {
                var debug = Environment.GetEnvironmentVariable("DEBUG");
                return debug != null && debug.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public static int Main(string[] args)
        {

            // Create servicecollection with di services
            var services = new ServiceCollection()
                .AddSingleton<Lib.Interfaces.IAppConfiguration, Lib.Services.JsonFileConfiguration>()
                .AddSingleton<Lib.Interfaces.IInstanceService, Lib.Services.MastodonInstanceService>()
                .AddSingleton<Lib.Interfaces.IFeedService, Lib.Services.FeedService>()
                .AddSingleton<Lib.Interfaces.ICacheService, Lib.Services.SqliteCache.SqliteCacheService>()
                .AddSingleton<IConsole>(PhysicalConsole.Singleton)
                .AddLogging(log =>
                {
                    // Add console logger
                    log.AddConsole(c => c.IncludeScopes = true);
                    log.AddSerilog();

                    if (Program.Debug)
                        log.SetMinimumLevel(LogLevel.Debug);
                })
                .BuildServiceProvider();

            var cfg = services.GetService<IAppConfiguration>();

            // Initialize serilog logger
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(cfg.ConfigurationRoot)
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithMachineName()
                .Enrich.FromLogContext()
                .CreateLogger();

            // Create application for configuration
            var app = new CommandLineApplication<Program>();

            // configure app
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);

            // run and return
            var result = app.Execute(args);
            services.Dispose();
            return result;
        }

        public Program(ILogger<Program> logger, IAppConfiguration configuration)
        {
            this.log = logger;
            this.cfg = configuration;
        }

        // Is executed when no arguments are given.
        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            using (log.BeginScope($"{ nameof(Program) }->{ nameof(OnExecute) }"))
            {
                app.ShowHelp();

                console.ForegroundColor = ConsoleColor.Red;
                console.Error.WriteLine("You must specify a subcommand.\n");
                console.ResetColor();

                return 1;
            }
        }
    }
}
