# FeedToMastodon

CommandLine client to read datafeeds and "toot" them to a mastodon instance.

## Status

### Done

- Can read RSS2.0 and Atom feeds
- Can communicate with mastodon server
- Publishes the feeds to mastodon
- Works with `windows/linux/osx` (and any other platform `dotnet core 2.1` runs)

### Todo

- Communicate errors to user
- Use logfiles
- Document usage (for now just quick documented)

## Requirements

### Run the software

- You need nothing preinstalled to run the software.
- The [releases](https://github.com/road42/FeedToMastodon/releases) are self contained and should not
  need any runtime.

### Change and compile

- At least `dotnet core sdk 2.1`

## How-to use

### First step - configfile location

Think about where to put your config file. Default is "feedtomastodon.cfg.json" in current
folder.

If you want to change this set an environment variable called `FEEDTOMASTODON_CFG` to the relative or absolute name.

```bash
export FEEDTOMASTODON_CFG="/home/<user>/.feedtomastodon.cfg.json"
```

### Second step - register with server

```bash
FeedToMastodon.Cli register --instance example.social
```

You will have to enter the email-address and the passwort of the
account you want to use once. You must disable Two-factor-auth for this. After the application-registration you may and should enable it agein.

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

#### DateFormaString

May contain everything from `DateTime.ToString()` in `dotNet`

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

I the existing projects available didn't solve my problems and I had some spare time.

## License

This software is licensed under the [MIT license](LICENSE).
