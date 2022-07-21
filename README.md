# SammBot.NET
SammBot rewritten for the fourth time, but in C#.

It has some hardcoded bits, like the OwnerUserId config entry. Should be easy to replace it by your own UID.

:warning: **JetBrains Mono Regular must be installed in your machine, otherwise the `viewhex` and `viewrgb` commands in the Utils module will NOT work!**

# Features
* **Administration Module**: Allows the bot's owner to make the bot say a message to a specific channel and guild. Also allows the owner to set variables in config.json, or restart/shutdown the bot, all remotely.
* **Janitoring Module**: Allows you to clear custom commands, quotes, or bulk delete messages.
* **Tags Module**: Allows users to create tags that make Samm-Bot reply with a custom message when used with the "tags get" command.
* **Fun Module**: Currently has a hug, pat, urban dictionary, dice, kill and magic 8-ball command.
* **Help Module**: Easy to use automatic help command that uses reflection and client information to display modules and commands.
* **Information Module**: Shows information about the bot (guilds, runtime information...) and can also show information about a user or server.
* **Nsfw Module**: (Hope you are happy, assholes) Searches rule34 for NSFW content. Must be ran under a channel marked as NSFW.
* **Quotes Module**: Stores some user messages into a database, that can be used to return random out-of-context quotes.
* **Random Module**: Allows users to retrieve random content from the cat, dog, fox or duck APIs, or a random SCP.
* **Profiles Module**: User profiles (pronouns).
* **Utils Module**: Allows you to ban, kick, or get the profile picture of a user, and more.
 
# Packages & Nuget
Visual Studio/dotnet should already detect the packages you need and install them.
Building should be straightforward.

***Attention!*** config.json and status.json files must be created manually! Templates for both are provided in the root of this repository.

***Attention!*** Use .NET 6.0!

# Building Databases
Grab the command line, and type in this command:

```
dotnet ef database update -c <name of DbContext class>
```

To update the databases. Grab them, and copy them to the build path so that the bot can access them.

# Code Style Guidelines

Please read the [style guidelines](STYLE_GUIDELINES.md) before creating a pull request.
