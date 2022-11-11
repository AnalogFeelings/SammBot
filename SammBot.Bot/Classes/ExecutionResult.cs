using Discord.Interactions;

namespace SammBot.Bot.Classes
{
    public class ExecutionResult : RuntimeResult
    {
        public ExecutionResult(InteractionCommandError? Error, string Reason) : base(Error, Reason) { }
        
        public static ExecutionResult FromError(string Reason) =>
            new ExecutionResult(InteractionCommandError.Unsuccessful, Reason);
        
        public static ExecutionResult Succesful() =>
            new ExecutionResult(null, "Execution succesful.");
    }
}
