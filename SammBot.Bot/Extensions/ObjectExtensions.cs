using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using SammBot.Bot.Attributes;

namespace SammBot.Bot.Extensions;

public static class ObjectExtensions
{
    public static string ToQueryString(this object TargetObject)
    {
        IEnumerable<string> formattedProperties = from p in TargetObject.GetType().GetProperties()
            where p.GetValue(TargetObject, null) != null
            where p.GetCustomAttribute<UglyName>() != null
            select p.GetCustomAttribute<UglyName>()!.Name + "=" + HttpUtility.UrlEncode(p.GetValue(TargetObject, null).ToString());

        return string.Join("&", formattedProperties.ToArray());
    }
}