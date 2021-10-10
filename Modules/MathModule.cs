using Discord.Addons.Interactive;
using Discord.Commands;
using SammBotNET.Services;
using System;
using System.Data;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Math")]
    [Group("math")]
    public class MathModule : InteractiveBase<SocketCommandContext>
    {
        public MathService MathService { get; set; }

        [Command("calculate", RunMode = RunMode.Async)]
        [Summary("Calculates the math expression from the parameter Expression.")]
        public async Task CalculateAsync([Remainder] string Expression)
        {
            if (MathService.IsDisabled)
            {
                await ReplyAsync($"The module \"{this.GetType().Name}\" is disabled.");
                return;
            }

            string authorid = Context.Message.Author.Id.ToString();
            try
            {
                double exprResult = Convert.ToDouble(new DataTable().Compute(Expression, null));

                await ReplyAsync($"<@{authorid}>, the result is: `{exprResult}`.");
            }
            catch (EvaluateException)
            {
                await ReplyAsync($"<@{authorid}>, invalid syntax." +
                    $" Make sure your expression doesn't contain letters, or operators other than +-/*%.");
                return;
            }
            catch (SyntaxErrorException)
            {
                await ReplyAsync($"<@{authorid}>, invalid math." +
                    $" Make sure your expression doesn't do multi-column things like in Microsoft Excel.");
                return;
            }
        }
    }
}
