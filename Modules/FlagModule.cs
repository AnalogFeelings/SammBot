/*using Discord;
using Discord.Commands;
using FlagPFP.Core.Loading;
using SammBotNET.Services;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Color = Discord.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace SammBotNET.Modules
{
    [Name("FlagPFP Integration")]
    [Group("flagpfp")]
    public class FlagModule : ModuleBase
    {
        public FlagService FlagsService { get; set; }

        [Command("flagify")]
        [Summary("Flagifies your profile picture.")]
        public async Task FlagifyAsync(int margin, params string[] flags)
        {
            if (!FlagsService.LoadedFlags) FlagsService.FlagMaker.LoadFlagDefsFromDir("Flag JSONs");
            int CurrentNumber = FlagsService.FlagNumber;
            FlagsService.FlagNumber++;

            foreach (string flag in flags)
            {
                if (!FlagsService.FlagMaker.FlagDictionary.ContainsKey(flag))
                {
                    await ReplyAsync($"The flag type \"{flag}\" does not exist!");
                    return;
                }
            }

            IUser user = await Context.Guild.GetUserAsync(Context.Message.Author.Id);
            SaveImage($"user{CurrentNumber}.png", user.GetAvatarUrl(size: 2048), ImageFormat.Png);

            Bitmap userBitmap = Bitmap.FromFile($"user{CurrentNumber}.png") as Bitmap;
            int innerSize = userBitmap.Width;
            int fullSize = userBitmap.Width;
            userBitmap.Dispose();

            FlagsService.FlagMaker.ExecuteProcessing($"user{CurrentNumber}.png", margin,
                innerSize, fullSize, $"output{CurrentNumber}.png", flags);

            EmbedBuilder builder = new EmbedBuilder
            {
                Color = Color.DarkPurple,
                Title = "FlagPFP",
                Description = "By Aesthetical#9203",
                ImageUrl = $"attachment://output{CurrentNumber}.png"
            };

            await Context.Channel.SendFileAsync($"output{CurrentNumber}.png", embed: builder.Build());

            File.Delete($"user{CurrentNumber}.png");
            File.Delete($"output{CurrentNumber}.png");
        }

        [Command("flags")]
        [Summary("Displays all available flags.")]
        public async Task FlagsAsync()
        {
            if (!FlagsService.LoadedFlags) FlagsService.FlagMaker.LoadFlagDefsFromDir("Flag JSONs");

            string fullReply = "Flag types are:\n```md\n";
            foreach (KeyValuePair<string, PrideFlag> flags in FlagsService.FlagMaker.FlagDictionary)
                fullReply += $"{flags.Value.ParameterName}\n";
            fullReply += "```";
            await ReplyAsync(fullReply);
        }

        public void SaveImage(string filename, string imageUrl, ImageFormat format)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(imageUrl);
            Bitmap bitmap = new Bitmap(stream);

            if (bitmap != null)
            {
                bitmap.Save(filename, format);
            }

            bitmap.Dispose();
            stream.Flush();
            stream.Close();
            client.Dispose();
        }
    }
}*/