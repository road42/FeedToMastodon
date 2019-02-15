# Logging

Logging is done with [Serilog](https://serilog.net/). There is support
for configure logging with the config-file.

Example:

```json
{
    "Serilog": {
        "Using": ["Serilog.Sinks.File"],
        "MinimumLevel": "Debug",
        "WriteTo": [
            {
                "Name": "File",
                "Args": {
                    "path": "/home/<user>/newsLog/log-.txt",
                    "rollingInterval": "Day"
                }
            }
        ]
    }
}
```

More information about the ["File"-sink](https://github.com/serilog/serilog-sinks-file)

## Added sinks

- [Serilog.Sinks.File](https://github.com/serilog/serilog-sinks-file)
- [Serilog.Sinks.RollingFile](https://github.com/serilog/serilog-sinks-rollingfile)
- [Serilog.Sinks.Console](https://github.com/serilog/serilog-sinks-console) for now, this
  duplicates the output. I will have to put some useful defaults here.