//Originally made by jalaljaleh for Dynastio.Discord
//Read the original file at https://github.com/jalaljaleh/Dynastio.Discord/blob/master/Dynastio.Bot/Interactions/Preconditions/RateLimit.cs
//Originally licensed under Apache 2.0 https://github.com/jalaljaleh/Dynastio.Discord/blob/master/LICENSE.txt
//
//Modifications:
//- Made it cleaner.
//- Adapted it for text-based commands.

using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Classes
{
    public class RateLimit : PreconditionAttribute
    {
        private static ConcurrentDictionary<ulong, List<RateLimitItem>> _Items = new ConcurrentDictionary<ulong, List<RateLimitItem>>();
        private readonly RateLimitType? _LimitType;
        public readonly int Requests;
        public readonly int Seconds;

        /// <summary>
        /// The constructor for the <see cref="RateLimit"/> class.
        /// </summary>
        /// <param name="Seconds">The amount of seconds that the user will have to wait to execute the command again once they are timed out.</param>
        /// <param name="Requests">The amount of times a user can execute a command until they hit the rate limit.</param>
        /// <param name="Context">The context of the rate limit.</param>
        public RateLimit(int Seconds, int Requests, RateLimitType Context = RateLimitType.User)
        {
            this._LimitType = Context;
            this.Requests = Requests;
            this.Seconds = Seconds;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, CommandInfo CommandInfo, IServiceProvider Services)
        {
            string command = CommandInfo.Module.Name + " " + CommandInfo.Name;
            DateTime dateNow = DateTime.UtcNow;
            ulong contextId = _LimitType.Value switch
            {
                RateLimitType.User => Context.User.Id,
                RateLimitType.Channel => Context.Channel.Id,
                RateLimitType.Guild => Context.Guild.Id,
                RateLimitType.Global => 0,
                _ => 0
            };

            List<RateLimitItem> targetItem = _Items.GetOrAdd(contextId, new List<RateLimitItem>());
            List<RateLimitItem> rateLimitItems = targetItem.Where(x => x.Command == command).ToList();

            foreach (RateLimitItem item in rateLimitItems)
            {
                if (dateNow >= item.ExpiresAt)
                    targetItem.Remove(item);
            }

            if (rateLimitItems.Count >= Requests)
            {
                int secondsLeft = (rateLimitItems.Last().ExpiresAt - dateNow).Seconds;

                return PreconditionResult.FromError($"This command is in cooldown! You can use it again in **{secondsLeft}** second(s).\n\n" +
                    $"The default cooldown for this command is **{Seconds}** second(s).\n" +
                    $"You can run this command **{Requests}** time(s) before hitting cooldown.");
            }

            targetItem.Add(new RateLimitItem()
            {
                Command = command,
                ExpiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(Seconds)
            });

            return PreconditionResult.FromSuccess();
        }

        private class RateLimitItem
        {
            public string Command { get; set; }
            public DateTime ExpiresAt { get; set; }
        }

        public enum RateLimitType
        {
            User,
            Channel,
            Guild,
            Global
        }
    }
}
