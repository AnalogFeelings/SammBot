global using SammBot.Bot.Core;
global using SammBot.Bot.Database;
global using SammBot.Bot.Extensions;
global using SammBot.Bot.RestDefinitions;
global using SammBot.Bot.Services;
global using LogSeverity = Matcha.LogSeverity;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Discord;

namespace SammBot.Bot.Core
{
    public class BotGlobals
    {
        public readonly Stopwatch StartupStopwatch = new Stopwatch();
        public readonly Stopwatch RuntimeStopwatch = new Stopwatch();

        public readonly AllowedMentions AllowOnlyUsers = new AllowedMentions(AllowedMentionTypes.Users);
        
        protected BotGlobals() {}
        
        public static void RestartBot()
        {
            string timeoutCommand = $"/C timeout 3 && {Environment.ProcessPath}";
            string executableCommand = "cmd.exe";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                timeoutCommand = $"-c \"sleep 3s && {Environment.ProcessPath}\"";
                executableCommand = "bash";
            }

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                Arguments = timeoutCommand,
                FileName = executableCommand,
                CreateNoWindow = true
            };
            Process.Start(startInfo);

            Environment.Exit(0);
        }
        
        private static BotGlobals _PrivateInstance;
        public static BotGlobals Instance
        {
            get
            {
                return _PrivateInstance ??= new BotGlobals();
            }
        }
    }
}