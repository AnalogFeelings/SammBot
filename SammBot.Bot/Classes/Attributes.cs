using System;

namespace SammBot.Bot.Classes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HideInHelp : Attribute
    {
        public HideInHelp() { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleEmoji : Attribute
    {
        public string Emoji = string.Empty;

        public ModuleEmoji(string Emoji) => this.Emoji = Emoji;
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class FullDescription : Attribute
    {
        public string Description;

        public FullDescription(string Description) => this.Description = Description;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class FullName : Attribute
    {
        public string Name;

        public FullName(string Name) => this.Name = Name;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class NotModifiable : Attribute
    {
        public NotModifiable() { }
    }
}
