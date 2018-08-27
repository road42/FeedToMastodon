# FeedToMastodon

[![Build Status Develop](https://travis-ci.com/road42/FeedToMastodon.svg?branch=develop)](https://travis-ci.com/road42/FeedToMastodon)
[![Build Status Master](https://travis-ci.com/road42/FeedToMastodon.svg?branch=master)](https://travis-ci.com/road42/FeedToMastodon)

CommandLine client to read datafeeds and "toot" them to a mastodon instance.

## Status and todo

See [issues](https://github.com/road42/FeedToMastodon/labels/enhancement)

## Requirements and download

- You need nothing preinstalled to run the software.
- The [releases](https://github.com/road42/FeedToMastodon/releases) are self contained and should not
  need any runtime.
- Download, unpack, configure, use

## Help? Bugs? Errors? But-I-Want-ToHaves!

I want to provide some help if you get errors or have feature requests. Create an issue, if you
have bugs. Provide as much information there as possible. Please don't include your clientId,clientSecret oder tokens.

I created an account for this: [@feedtomastodon@road42.social](mastodon://@feedtomastodon@road42.social) write me there.

## How-to use

- [Logging](docs/Logging.md)

### Step 0: Environment variables

There are two environment variables:

1. `DEBUG=true` is optional and tells the client to output debugLogs
2. `FEEDTOMASTODON_CFG` is **important**. This is the location of your `configFile`

### First step - configfile location

Think about where to put your config file. Default is "feedtomastodon.cfg.json" in current
folder.

If you want to change this set an environment variable called `FEEDTOMASTODON_CFG` to the relative or absolute name.

Protect and secure this file!

#### Example for Linux/MacOs

```bash
export FEEDTOMASTODON_CFG="/home/<user>/.feedtomastodon.cfg.json"
```

### Second step - register with server

```bash
FeedToMastodon.Cli register --instance example.social
```

You will have to enter the email-address and the passwort of the
account you want to use once. You must disable twofactor-authentication for this. After the application registration you may and should enable it again.

This command creates the `configFile`, registers the application with the instance and create an `accessToken` for later use.

Your credentials aren't saved anywhere and only sent to the instance-server to create a new `accessToken`.

Now your config looks something like that:

```json
{
  "Instance": {
    "Name": "example.social",
    "ClientId": "<longString>",
    "ClientSecret": "<longString>",
    "AccessToken": "<longString>"
  },
  "Cache": {
    "ConnectionString": "/home/<user>/.feedtomastodon.cache.db",
    "MaxEntries": 1000,
    "MaxAge": "30.00:00:00"
  },
  "Toot": {
    "Visibiliy": "Public",
    "Template": "{feedname}: {title} - {published} ({link}) #news #rss",
    "DateFormatString": "yyyy-MM-dd HH:mm"
  },
  "Feeds": [
  ]
}
```

### Third step - Configure `Toot` layout

The toots can be configured like this:

```json
"Toot": {
    "Visibiliy": "Public",
    "Template": "{feedname}: {title} - {published} ({link}) #news #rss",
    "DateFormatString": "yyyy-MM-dd HH:mm"
}
```

#### Templatefields

Valid fields are:
- `{feedname}`
- `{id}`
- `{title}`
- `{summary}` (Atom only)
- `{description}`
- `{link}`
- `{published}`
- `{lastUpdated}`

#### Visibility

Valid values are:
- `Public`
- `Direct`
- `Unlisted`
- `Private`

#### DateFormatString

May contain everything from `DateTime.ToString()` in [dotNet](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings)

### Fourth step - Add feeds

#### RSS

A rss feed entry looks like this:

```json
{
    "Name": "<yourFeedName>",
    "Type": "RSS",
    "ConnectionString": "<url>"
}
```

#### Atom
An atom feed entry looks like this:
```json
{
    "Name": "<yourFeedName>",
    "Type": "Atom",
    "ConnectionString": "<url>"
}
```

#### Combined

put some of them into `Feeds`:
```json
"Feeds": [
    {
        "Name": "<yourFeedName>",
        "Type": "RSS",
        "ConnectionString": "<url>"
    },
    {
        "Name": "<yourFeedName>",
        "Type": "Atom",
        "ConnectionString": "<url>"
    }
]
```

#### Overwrite `toot` defaults for feed

Just add a `Toot` object to your feed:

```json
{
    "Name": "<yourFeedName>",
    "Type": "RSS",
    "ConnectionString": "<url>",
    "Toot": {
        "Visibiliy": "Public",
        "Template": "{feedname}: {title} - {published} ({link})",
        "DateFormatString": "yyyy-MM-dd HH:mm"
    }
}
```

### Fifth step - run

```
FeedToMastodon.Cli run
```

## Why?

The existing projects available didn't solve my problems and I had some spare time.

## But, why dotNet?

Because I can.

## License

This software is licensed under the [MIT license](LICENSE).
