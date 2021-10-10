# BaldiBot.NET
 BaldiBot rewritten for the third time, but in C#. Code is messy right now since I began the project yesterday.
 
 <h1>Packages & Nuget</h1>
 Visual Studio should already detect the packages you need and install them, but if it doesn't, here are the packages needed:
 
 * Pastel
 * Figgle
 * Discord.NET
 * Microsoft.Extensions.DependencyInjection
 * Microsoft.Extensions.Configuration
 * Microsoft.EntityFrameworkCore.Design
 * dotnet-ef (dotnet tool install --global dotnet-ef)

<h1>Building Databases</h1>
Grab the command line, and type in this command:

```
dotnet ef database update -c <name of DbContext class>
```

To update the databases. Grab them, and copy them to the build path so that the bot can access them.
