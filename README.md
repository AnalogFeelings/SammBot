![sammbot](SammBot.svg)
# Samm-Bot
Samm-Bot rewritten for the fourth time, but in C# and [Discord.Net](https://github.com/discord-net/Discord.Net) instead of TypeScript and [discord.js](https://github.com/discordjs/discord.js). In constant evolution!  
It uses [Matcha](https://github.com/AestheticalZ/Matcha) for event logging!

This README file is not updated often, so sometimes it's possible for new commands or modules to be added without them being listed here.

## Features
* :gear: **Bot Administration Module**: Allows the bot's owner to manage the bot remotely.
* :judge: **Moderation Module**: Allows you to kick, ban, mute, or bulk delete messages.
* :label: **Tags Module**: Allows users to create tags that make Samm-Bot reply with a custom message when used with the "tags get" command.
* :game_die: **Fun Module**: Hugging, patting, asking the magic 8-ball, and more!
* :information_source: **Information Module**: Shows information about the bot and can also show information about a user or server.
* :slot_machine: **Random Module**: Allows users to retrieve random content from the cat, dog, fox or duck APIs, or a random SCP.
* :busts_in_silhouette: **Profiles Module**: User profiles (only pronouns for now).
* :wrench: **Utils Module**: Allows you to view RGB or HEX colors, get a user's avatar, or the weather forecast of your city.
 
## Packages & Nuget
Visual Studio/dotnet should already detect the packages you need and install them.
Building should be straightforward.

:warning: **The file config.json must be filled in manually! Run the bot, and it will create an empty config.json file for you.**

:warning: **JetBrains Mono Regular is needed for the `viewhex` and `viewrgb` commands in the Utils module to work!**

## Handling Databases

### Creating Migrations
Grab the command line, cd to the Source folder, and execute this command:

```
dotnet ef migrations add <MigrationName> --project ../SammBotNET.csproj
```

This will let you add columns to existing databases.

### Applying Migrations
Grab the command line, cd to the Source folder, and execute this command:

```
dotnet ef database update --project ../SammBotNET.csproj
```

This will apply the latest migration to the database.

## Code Style Guidelines

Please read the [style guidelines](STYLE_GUIDELINES.md) before creating a pull request.

# License
© Copyright 2022 AestheticalZ.  
Licensed under the [GPL v3.0 license](LICENSE)

# Special Thanks
| Logo | Message |
| ---- | ------- |
| <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png" alt="JetBrains" width="128"/> | Thanks to **JetBrains** for providing an OSS license for their products! |
