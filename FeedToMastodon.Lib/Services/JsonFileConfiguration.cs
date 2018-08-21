/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE.txt file in the project root for more information.
*/

using System;
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
        Models.Configuration.Application configuration = new Models.Configuration.Application();

        public Models.Configuration.Application GetConfiguration()
        {
            return configuration?? new Models.Configuration.Application();
        }

        /*
            Initializes configuration and reads the configfile.

            If the configfile does not exist a new one is created.
        */
        public bool InitializeFromConnectionString(string connectionString)
        {
            try
            {
                var fileInfo = new FileInfo(connectionString);

                // Configuration file does not exists yet => create it from empty cfg-object
                if (!fileInfo.Exists)
                {
                    var app = new Models.Configuration.Application();

                    // serialize JSON to a string and then write string to a file
                    File.WriteAllText(connectionString, JsonConvert.SerializeObject(app, Formatting.Indented));
                }

                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(fileInfo.FullName, false, true);

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

        public bool IsValidConnectionString(string connectionString)
        {
            // No need to check if empty
            if (string.IsNullOrEmpty(connectionString))
                return false;

            // Create fileInformation object from name
            // the file does not have to exist to do this.
            System.IO.FileInfo fileInfo = null;

            try
            {
                // Create fileInfo ... this will fail on invalid filenames
                fileInfo = new FileInfo(connectionString);
            }
            // catch exceptions so fileInfo stays null
            catch (Exception) { }

            // false if fileInfo is null
            return (fileInfo is null) ? false : true;
        }
    }
}