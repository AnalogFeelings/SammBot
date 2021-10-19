# SammBot.NET
SammBot rewritten for the fourth time, but in C#.
 
# Packages & Nuget
Visual Studio/dotnet should already detect the packages you need and install them.

# Building Databases
Grab the command line, and type in this command:

```
dotnet ef database update -c <name of DbContext class>
```

To update the databases. Grab them, and copy them to the build path so that the bot can access them.
