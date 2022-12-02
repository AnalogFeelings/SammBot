using System;

namespace SammBot.Bot.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class PrettyName : Attribute
{
    public readonly string Name;

    public PrettyName(string Name) => this.Name = Name;
}