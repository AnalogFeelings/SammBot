using Discord;
using Discord.Commands;
using SammBotNET.FlagPFP.Loading;
using SammBotNET.FlagPFP.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("FlagPFP Integration")]
    [Group("flagpfp")]
    public class FlagPFPModule : ModuleBase
    {
        [Command("flagify")]
        [Summary("Flagifies the user you provide, you if you don't provide a user.")]
        public async Task FlagifyAsync(string type, int innerSize, int margin, ulong uUser = 0)
        {
            FlagLoader flagLoader = new FlagLoader();
            Dictionary<string, PrideFlag> flagDictionary = flagLoader.LoadFlags("Flag JSONs");
            if (!flagDictionary.TryGetValue(type, out PrideFlag flagPath))
            {
                string fullReply = "Invalid flag type! Flag types are:\n```md\n";
                foreach (KeyValuePair<string, PrideFlag> flags in flagDictionary)
                    fullReply += $"[-{flags.Value.ConsoleHeader}-]({flags.Value.ParameterName})\n";
                fullReply += "```";
                await ReplyAsync(fullReply);
                return;
            }

            IUser user;
            if (uUser != 0)
            {
                user = await Context.Guild.GetUserAsync(uUser);
            }
            else
            {
                user = await Context.Guild.GetUserAsync(Context.Message.Author.Id);
            }
            SaveImage("user.png", user.GetAvatarUrl(size: 512), System.Drawing.Imaging.ImageFormat.Png);

            Bitmap imageFile = null;
            Bitmap flag = null;
            ImageProcessing processor = new ImageProcessing();

            try
            {
                imageFile = processor.LoadAndResizeBmp("user.png", 800, 800);
                flag = processor.LoadAndResizeBmp("Flags/" + flagPath.FlagFile, 800, 800);
            }
            catch (Exception ex)
            {
                await ReplyAsync("Something went wrong when executing FlagPFP (Load)!\n" + ex.ToString());
                imageFile.Dispose();
                flag.Dispose();
                return;
            }

            Bitmap croppedBitmap = processor.CropPicture(ref imageFile, 800, false);
            Bitmap flagBitmap = processor.CropFlag(ref flag, margin);
            Bitmap finalBitmap = processor.StitchTogether(ref flagBitmap, ref croppedBitmap, innerSize);

            try { finalBitmap.Save("output.png", System.Drawing.Imaging.ImageFormat.Png); }
            catch (Exception ex)
            {
                await ReplyAsync("Something went wrong when executing FlagPFP (Save)!\n" + ex.ToString());
                imageFile.Dispose();
                flag.Dispose();
                return;
            }

            string reply = "__FlagPFP__, by **Aesthetical**. <https://github.com/AestheticalZ/FlagPFP>";
            if (!string.IsNullOrEmpty(flagPath.DesignCredits)) reply += $"\n***{flagPath.DesignCredits}***";
            await Context.Message.Channel.SendFileAsync("output.png",
                reply);
            croppedBitmap.Dispose();
            flagBitmap.Dispose();
            finalBitmap.Dispose();
            imageFile.Dispose();
            flag.Dispose();
        }

        [Command("flags")]
        [Summary("Displays all available flags.")]
        public async Task FlagsAsync()
        {
            FlagLoader flagLoader = new FlagLoader();
            Dictionary<string, PrideFlag> flagDictionary = flagLoader.LoadFlags("Flag JSONs");
            string fullReply = "Flag types are:\n```md\n";
            foreach (KeyValuePair<string, PrideFlag> flags in flagDictionary)
                fullReply += $"[-{flags.Value.ConsoleHeader}-]({flags.Value.ParameterName})\n";
            fullReply += "```";
            await ReplyAsync(fullReply);
        }

        public void SaveImage(string filename, string imageUrl, System.Drawing.Imaging.ImageFormat format)
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
}
