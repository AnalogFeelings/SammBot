using System.ComponentModel.DataAnnotations;

namespace SammBot.Bot.Database;

public class Pronoun
{
    [Key]
    public ulong UserId { get; set; }
    public string Subject { get; set; }
    public string Object { get; set; }
    public string DependentPossessive { get; set; }
    public string IndependentPossessive { get; set; }
    public string ReflexiveSingular { get; set; }
    public string ReflexivePlural { get; set; }
}