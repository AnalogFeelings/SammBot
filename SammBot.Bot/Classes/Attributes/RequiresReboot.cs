using System;

namespace SammBot.Bot.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class RequiresReboot : Attribute
{
    public RequiresReboot() { }
}