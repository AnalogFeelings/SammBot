using System;

namespace SammBot.Bot.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class DetailedDescription : Attribute
{
    public readonly string Description;

    public DetailedDescription(string Description) => this.Description = Description;
}