using System.Collections.Generic;

namespace SammBotNET.Classes
{
	public class AutodeqList<T> : LinkedList<T>
	{
		private readonly int MaxSize;

		public AutodeqList(int MaxSize) => this.MaxSize = MaxSize;

		public void Push(T Item)
		{
			this.AddFirst(Item);

			if (this.Count > MaxSize) this.RemoveLast();
		}
	}
}
