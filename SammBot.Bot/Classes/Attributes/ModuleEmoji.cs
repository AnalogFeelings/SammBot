using System;

namespace SammBot.Bot.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ModuleEmoji : Attribute
{
    public readonly string Emoji;

    public ModuleEmoji(string Emoji) => this.Emoji = Emoji;
}