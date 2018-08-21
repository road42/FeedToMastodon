/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE.txt file in the project root for more information.
*/

using System;
using System.ComponentModel.DataAnnotations;
using FeedToMastodon.Lib.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace FeedToMastodon.Cli
{
    // Default/Main Command.
    // Is executed, when no options or commands are given
    //
    // Also contains some static default variables
    // and the registration of dependency injection
    [Command(
        Name = Program.PROGRAMNAME,
        FullName = Program.PROGRAMFULLNAME,
        Description = "Pushes datafeeds to mastodon.",
        ExtendedHelpText = Program.EXTENDEDHELPTEXT
    )]
    [Subcommand("register", typeof(Commands.RegisterApplication))]
    [HelpOption]
    class Program
    {
        public const string PROGRAMNAME = "FeedToMastodon";
        public const string PROGRAMFULLNAME = "FeedToMastodon (Version: 0.0.1)";
        public const string EXTENDEDHELPTEXT = "\nSee more information about the software at: https://github.com/road42/FeedToMastodon \n(c) 2018 Christoph Jahn - Licensed under MIT license (See LICENSE)\n";

        public const string APPNAME = "FeedToMastodon";
        public const string APPSITE = "https://github.com/road42/FeedToMastodon";

        public static int Main(string[] args)
        {
            // Create servicecollection with di services
            // For now "only" with dummyServices
            var services = new ServiceCollection()
                .AddSingleton<IAppConfiguration, Lib.Services.JsonFileConfiguration>()
                .AddSingleton<IInstanceService, Lib.Services.Dummy.InstanceService>()
                .AddSingleton<IFeedService, Lib.Services.Dummy.FeedService>()
                .AddSingleton<IConsole>(PhysicalConsole.Singleton)
                .BuildServiceProvider();

            // Create application for configuration
            var app = new CommandLineApplication<Program>();

            // configure app
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);

            // run and return
            return app.Execute(args);
        }

        // Is executed when no arguments are given.
        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            app.ShowHelp();

            console.ForegroundColor = ConsoleColor.Red;
            console.Error.WriteLine("You must specify a subcommand.\n");
            console.ResetColor();

            return 1;
        }
    }
}
