//Originally made by jalaljaleh for Dynastio.Discord
//Read the original file at https://github.com/jalaljaleh/Dynastio.Discord/blob/master/Dynastio.Bot/Interactions/Preconditions/RateLimit.cs
//Originally licensed under Apache 2.0 https://github.com/jalaljaleh/Dynastio.Discord/blob/master/LICENSE.txt
//
//Modifications:
//- Made it cleaner.
//- Adapted it for text-based commands.

using System.Collections.Concurrent;
using Discord;
using Discord.Interactions;

namespace SammBot.Library.Preconditions;

/// <summary>
/// A precondition that prevents people from running commands too often.
/// </summary>
public class RateLimit : PreconditionAttribute
{
    private static ConcurrentDictionary<ulong, List<RateLimitItem>> _Items = new ConcurrentDictionary<ulong, List<RateLimitItem>>();
    
    /// <summary>
    /// The amount of times a user can execute a command before being timed out.
    /// </summary>
    public readonly int RequestLimit;
    
    /// <summary>
    /// The time in seconds that the timeout will last.
    /// </summary>
    public readonly int TimeoutDuration;

    /// <summary>
    /// The constructor for the <see cref="RateLimit"/> class.
    /// </summary>
    /// <param name="TimeoutDuration">The amount of seconds that the user will have to wait to execute the command again once they are timed out.</param>
    /// <param name="RequestLimit">The amount of times a user can execute a command until they hit the rate limit.</param>
    public RateLimit(int TimeoutDuration, int RequestLimit)
    {
        this.RequestLimit = RequestLimit;
        this.TimeoutDuration = TimeoutDuration;
    }
        
    /// <summary>
    /// Checks if the user executing the command is timed out or not.
    /// </summary>
    /// <param name="Context">The command's interaction context.</param>
    /// <param name="CommandInfo">The command's information.</param>
    /// <param name="Services">The bot's service provider.</param>
    /// <returns>A successful <see cref="PreconditionResult"/> if the user is not timed out.</returns>
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext Context, ICommandInfo CommandInfo, IServiceProvider Services)
    {
        string command = CommandInfo.Module.Name + " " + CommandInfo.Name;
        DateTime dateNow = DateTime.UtcNow;

        List<RateLimitItem> targetItem = _Items.GetOrAdd(Context.User.Id, new List<RateLimitItem>());
        List<RateLimitItem> rateLimitItems = targetItem.Where(x => x.Command == command).ToList();

        foreach (RateLimitItem item in rateLimitItems)
        {
            if (dateNow >= item.ExpiresAt)
                targetItem.Remove(item);
        }

        if (rateLimitItems.Count > RequestLimit)
        {
            int secondsLeft = (rateLimitItems.Last().ExpiresAt - dateNow).Seconds;

            return Task.FromResult(PreconditionResult.FromError($"This command is in cooldown! You can use it again in **{secondsLeft}** second(s).\n\n" +
                                                                $"The default cooldown for this command is **{TimeoutDuration}** second(s).\n" +
                                                                $"You can run this command **{RequestLimit}** time(s) before hitting cooldown."));
        }

        targetItem.Add(new RateLimitItem()
        {
            Command = command,
            ExpiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(TimeoutDuration)
        });

        return Task.FromResult(PreconditionResult.FromSuccess());
    }

    private class RateLimitItem
    {
        public string Command { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}