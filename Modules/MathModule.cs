using Discord.Commands;
using SammBotNET.Extensions;
using SammBotNET.Services;
using System;
using System.Data;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Math")]
    [Summary("Simple calculator.")]
    [Group("math")]
    public class MathModule : ModuleBase<SocketCommandContext>
    {
        public MathService MathService { get; set; }

        [Command("calculate", RunMode = RunMode.Async)]
        [Alias("calc", "do")]
        [Summary("Calculates the math expression from the parameter Expression.")]
        public async Task<RuntimeResult> CalculateAsync([Remainder] string Expression)
        {
            if (MathService.IsDisabled)
                return ExecutionResult.FromError($"The module \"{nameof(MathModule)}\" is disabled.");

            string authorid = Context.Message.Author.Id.ToString();
            try
            {
                double exprResult = Convert.ToDouble(new DataTable().Compute(Expression, null));

                await ReplyAsync($"<@{authorid}>, the result is: `{exprResult}`.");
            }
            catch (EvaluateException)
            {
                return ExecutionResult.FromError($"<@{authorid}>, invalid syntax." +
                    $" Make sure your expression doesn't contain letters, or operators other than +-/*%.");
            }
            catch (SyntaxErrorException)
            {
                return ExecutionResult.FromError($"<@{authorid}>, invalid math." +
                    $" Make sure your expression doesn't do multi-column things like in Microsoft Excel.");
            }

            return ExecutionResult.Succesful();
        }
    }
}
