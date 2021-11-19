# SammBot.NET
SammBot rewritten for the fourth time, but in C#.

It has some hardcoded bits, like the AestheticalUid and SkylerUid config entries. Should be easy to replace them by your own UID or remake the system entirely.

# Features
* Administration Module: Allows the bot's owner to make the bot say a message to a specific channel and guild. Also allows the owner to set variables in config.json remotely.
* Janitoring Module: Allows you to clear custom commands, quotes, or bulk delete messages.
* Custom Commands Module: Allows users to create custom commands that make Samm-Bot reply with a custom message.
* Emotional Support Module: Awww, ignore this, its for my girlfriend...
* Fun Module: Currently has a hug, pat, urban dictionary and magic 8-ball command.
* Help Module: Easy to use automatic help command that uses reflection and client information to display modules and commands.
* Information Module: Shows information about the bot (guilds, runtime information...) and can also show information about a user.
* Nsfw Module: (Hope you are happy, assholes) Searches rule34 for NSFW content. Must be ran under a channel marked as NSFW.
* Quotes Module: Stores some user messages into a database, that can be used to return wacky random quotes.
* Random Module: Allows users to retrieve random content from the cat or dog APIs, or a random SCP.
* Profiles Module: User profiles (pronouns).
 
# Packages & Nuget
Visual Studio/dotnet should already detect the packages you need and install them.
Building should be straightforward.

***Attention!*** config.json and status.xml files must be created manually! Templates for both are provided in the root of this repository.

***Attention!*** Use .NET 6.0!

# Building Databases
Grab the command line, and type in this command:

```
dotnet ef database update -c <name of DbContext class>
```

To update the databases. Grab them, and copy them to the build path so that the bot can access them.
