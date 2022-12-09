using System;

namespace SammBot.Bot.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class HiddenSetting : Attribute
{
    public HiddenSetting() {}
}