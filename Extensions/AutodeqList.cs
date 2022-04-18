using System.Collections.Generic;

namespace SammBotNET.Extensions
{
	public class AutodeqList<T> : LinkedList<T>
	{
		private readonly int MaxSize;

		public AutodeqList(int MaxSize) => this.MaxSize = MaxSize;

		public void Push(T item)
		{
			this.AddFirst(item);

			if (this.Count > MaxSize) this.RemoveLast();
		}
	}
}
