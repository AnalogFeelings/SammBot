using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SammBot.Bot.Classes
{
    public static class ReflectionEnumerator
    {
        public static IEnumerable<T> GetChildrenOfType<T>() where T : class
        {
            List<T> foundClasses = new List<T>();

            foreach (Type type in Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(ClassType => ClassType.IsClass && !ClassType.IsAbstract && ClassType.IsSubclassOf(typeof(T))))
            {
                foundClasses.Add((T)Activator.CreateInstance(type));
            }

            return foundClasses;
        }
    }
}
