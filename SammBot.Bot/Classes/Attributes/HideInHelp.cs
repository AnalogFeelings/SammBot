using System;

namespace SammBot.Bot.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class HideInHelp : Attribute
{
    public HideInHelp() { }
}