using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SammBotNET.Extensions
{
	public static class ReflectionEnumerator
	{
		public static IEnumerable<T> GetChildrenOfType<T>() where T : class
		{
			List<T> FoundClasses = new List<T>();

			foreach (Type Type in Assembly.GetAssembly(typeof(T)).GetTypes()
				.Where(ClassType => ClassType.IsClass && !ClassType.IsAbstract && ClassType.IsSubclassOf(typeof(T))))
			{
				FoundClasses.Add((T)Activator.CreateInstance(Type));
			}

			return FoundClasses;
		}
	}
}
