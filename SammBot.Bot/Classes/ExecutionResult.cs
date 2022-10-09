using Discord.Commands;

namespace SammBotNET.Classes
{
    public class ExecutionResult : RuntimeResult
    {
        public ExecutionResult(CommandError? Error, string Reason) : base(Error, Reason) { }
        
        public static ExecutionResult FromError(string Reason) =>
            new ExecutionResult(CommandError.Unsuccessful, Reason);
        
        public static ExecutionResult Succesful() =>
            new ExecutionResult(null, "Execution succesful.");
    }
}
