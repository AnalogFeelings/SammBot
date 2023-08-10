[i14]: https://github.com/AnalogFeelings/SammBot/issues/14

<div align="center">
  <img src="Branding/SammBot.svg" width="256" height="256">
  <h1>
    :leaves: Samm-Bot
  </h1>
  
  ![Lines of code](https://img.shields.io/tokei/lines/github/analogfeelings/sammbot?label=Lines%20Of%20Code&style=flat-square)
  ![.NET Version](https://img.shields.io/badge/.NET%20Version-7-success?style=flat-square)
  [![GitHub issues](https://img.shields.io/github/issues/analogfeelings/sammbot?label=Issues&style=flat-square)](https://github.com/AnalogFeelings/sammbot/issues)
  [![GitHub pull requests](https://img.shields.io/github/issues-pr/analogfeelings/sammbot?label=Pull%20Requests&style=flat-square)](https://github.com/AnalogFeelings/sammbot/pulls)
  [![GitHub](https://img.shields.io/github/license/analogfeelings/sammbot?label=License&style=flat-square)](https://github.com/AnalogFeelings/sammbot/blob/master/LICENSE)
  [![GitHub commit activity (branch)](https://img.shields.io/github/commit-activity/m/analogfeelings/sammbot/master?label=Commit%20Activity&style=flat-square)](https://github.com/AnalogFeelings/sammbot/graphs/commit-activity)
  [![GitHub Repo stars](https://img.shields.io/github/stars/analogfeelings/sammbot?label=Stargazers&style=flat-square)](https://github.com/AnalogFeelings/sammbot/stargazers)
  [![Mastodon Follow](https://img.shields.io/mastodon/follow/109309123442839534?domain=https%3A%2F%2Ftech.lgbt&style=social)](https://tech.lgbt/@analog_feelings)
</div>

Samm-Bot rewritten for the fourth time, but in C# and [Discord.Net](https://github.com/discord-net/Discord.Net) instead of TypeScript and [discord.js](https://github.com/discordjs/discord.js). In constant evolution!  
It uses [Matcha](https://github.com/AnalogFeelings/Matcha) for event logging!

This README file is not updated often, so sometimes it's possible for new commands or modules to be added without them being listed here.

## :grey_question: What is Samm-Bot?

Samm-Bot is a Discord bot that I began developing around 2020 for fun.  
Its source code was private, was written in TypeScript, and was very feature-lacking.

In 2021, I decided to rewrite it in .NET, open source it and make it much more feature rich. Nowadays, it focuses on being useful, with features such as moderation and fun commands.

My main motivation is because currently, most large bots are owned by money-hungry people who like to shove Premium ads everywhere they can.  
I wanted to make a free and open source bot that people could rely on for simple server-keeping tasks such as warnings, logging and more.

> [!IMPORTANT]  
> Currently, the bot is in a **beta** state and is very unpolished.

## :trophy: Maintainers

This is a list of all the people who maintain Samm-Bot's development.

* :floppy_disk: Analog Feelings - Creator and lead developer.

## :books: Features

* :floppy_disk: **Bot Administration Module**: Allows the bot's owner to manage the bot remotely.
* :gear: **Server Settings Module**: Allows you to set server-specific settings, like logging, welcome messages, etc...
* :judge: **Moderation Module**: Allows you to kick, ban, mute, or bulk delete messages.
* :label: **Tags Module**: Allows users to create tags that make Samm-Bot reply with a custom message when used with the "tags get" command.
* :game_die: **Fun Module**: Hugging, patting, asking the magic 8-ball, and more!
* :information_source: **Information Module**: Shows information about the bot and can also show information about a user or server.
* :slot_machine: **Random Module**: Retrieve random content from the cat, dog, fox or duck APIs.
* :wrench: **Utils Module**: Allows you to view RGB or HEX colors, get a user's avatar, or the weather forecast of your city.
* :underage: **NSFW Module**: Allows you to retrieve NSFW (or SFW) images from popular sites such as rule34.
 
## :package: Building

The .NET toolset will automatically pull all the required NuGet packages on build.  
Samm-Bot has currently been tested on **x86-64** and **aarch64**.

For Linux and macOS users, this [tiny script](https://gist.github.com/AnalogFeelings/7969c2af2f87d606b3fd8b72cd8c6432) should make it easier for you to pull and build automatically.

> [!WARNING]  
> The file config.json must be filled in manually! Run the bot, and it will create a template config.json file for you.

> [!IMPORTANT]  
> ([#14][i14]) JetBrains Mono Regular is needed for the `viewhex` and `viewrgb` commands in the Utils module to work!

## :cd: Database Setup

Here is a small TL;DR on how to update the database in Samm-Bot.  
Knowing how EntityFramework works is still recommended for this project.

### :eight_spoked_asterisk: Creating Migrations
Grab the command line, cd to the SammBot.Bot folder, and execute this command:

```
dotnet ef migrations add <MigrationName>
```

This will let you modify the structure of the database's tables.

### :white_check_mark: Applying Migrations
Grab the command line, cd to the SammBot.Bot folder, and execute this command:

```
dotnet ef database update
```

This will apply the latest migration to the local database, or create a database if one doesn't already exist.

## :judge: Code Style Guidelines

Please read the [style guidelines](STYLE_GUIDELINES.md) before creating a pull request.  
Your pull request won't be merged until the code is up to the standards.

Adding an automatic `.editorconfig` file to enforce these is of low priority right now, so please read the file thoroughly.

## :balance_scale: License

© Copyright 2021-2023 Analog Feelings.  
Licensed under the [GPL v3.0 license](LICENSE).

## :sparkles: Special Thanks

This is a list of companies or projects who have been very helpful for the development of Samm-Bot.  
If your project or company has been very influential in the development of Samm-Bot, it will be listed here.

<div align="center">
  
  | Logo | Message |
  | ---- | ------- |
  | <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png" alt="JetBrains" width="128"/> | Thanks to **JetBrains** for providing an OSS license for their products! |
</div>
