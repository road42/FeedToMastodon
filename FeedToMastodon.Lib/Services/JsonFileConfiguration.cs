/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;
using System.Collections;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace FeedToMastodon.Lib.Services
{
    /*
        A JSON-implementation of the IAppConfiguration. Uses
        a filename as connectionString.
    */
    public class JsonFileConfiguration : Interfaces.IAppConfiguration
    {
        private Models.Configuration.Application configuration = new Models.Configuration.Application();

        // Returns the connectionString from environment or
        // the default one
        private string ConnectionString {
            get {
                return Environment
                    .GetEnvironmentVariable(Constants.ENVIRONMENT_CONFIGCONNECTIONSTRING_NAME) ??
                        Constants.DEFAULT_CONFIGCONNECTIONSTRING;
            }
        }

        // Read an initialize config
        public JsonFileConfiguration()
        {
            InitializeFromConnectionString();
        }

        public Models.Configuration.Application GetConfiguration()
        {
            return configuration?? new Models.Configuration.Application();
        }

        /*
            Initializes configuration and reads the configfile.
            If the configfile does not exist a new one is created.
        */
        private bool InitializeFromConnectionString()
        {
            // No need to check if empty
            if (string.IsNullOrEmpty(ConnectionString))
                return false;

            // Create fileInformation object from name
            // the file does not have to exist to do this.
            System.IO.FileInfo fileInfo = null;

            // Try to initialize fileInfo
            try
            {
                // Create fileInfo ... this will fail on invalid filenames
                fileInfo = new FileInfo(ConnectionString);
            }
            // catch exceptions so fileInfo stays null
            catch (Exception) { }

            // false if fileInfo is null
            if (fileInfo == null)
                return false;

            // Now initialize and read the file
            try
            {
                // Configuration file does not exist yet => create it from empty cfg-object
                if (!fileInfo.Exists)
                {
                    var app = new Models.Configuration.Application();

                    // serialize JSON to a string and then write string to a file
                    File.WriteAllText(ConnectionString, JsonConvert.SerializeObject(app, Formatting.Indented));
                }

                // Create and configure the ConfigurationBuilder;
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(fileInfo.FullName, false, true);

                // Read the file and bind to configuration-object
                builder
                    .Build()
                    .Bind(this.configuration);

            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                // TODO: Give error to user, without implementing parts of CommandLineClass in Lib
                return false;
            }

            return true;
        }

        public bool Save()
        {
            try {
                File.WriteAllText(ConnectionString, JsonConvert.SerializeObject(this.configuration, Formatting.Indented));
            } catch {
                // TODO: UserNotify
                return false;
            }

            return true;
        }
    }
}