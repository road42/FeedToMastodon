/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;
using System.ComponentModel.DataAnnotations;
using FeedToMastodon.Lib;
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
        Name = Constants.PROGRAMNAME,
        FullName = Constants.PROGRAMFULLNAME,
        Description = "Pushes datafeeds to mastodon.",
        ExtendedHelpText = Constants.EXTENDEDHELPTEXT
    )]
    [Subcommand("register", typeof(Commands.RegisterApplication))]
    [HelpOption]
    class Program
    {
        public static int Main(string[] args)
        {
            // Create servicecollection with di services
            // For now "only" with dummyServices
            var services = new ServiceCollection()
                .AddSingleton<Lib.Interfaces.IAppConfiguration, Lib.Services.JsonFileConfiguration>()
                .AddSingleton<Lib.Interfaces.IInstanceService, Lib.Services.Dummy.InstanceService>()
                .AddSingleton<Lib.Interfaces.IFeedService, Lib.Services.Dummy.FeedService>()
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
