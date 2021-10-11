using Discord.Commands;

namespace SammBotNET.Extensions
{
    public class ExecutionResult : RuntimeResult
    {
        public ExecutionResult(CommandError? error, string reason) : base(error, reason) { }
        public static ExecutionResult FromError(string reason) =>
            new(CommandError.Unsuccessful, reason);
        public static ExecutionResult Succesful() =>
            new(null, "Execution succesful.");
    }
}
