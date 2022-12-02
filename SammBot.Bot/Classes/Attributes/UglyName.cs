using System;

namespace SammBot.Bot.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class UglyName : Attribute
{
    public readonly string Name;

    public UglyName(string Name) => this.Name = Name;
}