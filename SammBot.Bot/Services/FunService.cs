using System.Net.Http;

namespace SammBot.Bot.Services;

public class FunService
{
    public readonly HttpClient FunHttpClient = new HttpClient();
}