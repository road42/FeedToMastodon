/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE.txt file in the project root for more information.
*/

using System;
using System.ComponentModel.DataAnnotations;
using FeedToMastodon.Lib.Interfaces;
using mastodon;
using mastodon.Enums;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace FeedToMastodon.Cli
{
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
                .AddSingleton<IAppConfiguration, Lib.Services.Dummy.AppConfiguration>()
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

        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("You must specify a subcommand.");
            app.ShowHelp();
            return 1;
        }
    }
}
