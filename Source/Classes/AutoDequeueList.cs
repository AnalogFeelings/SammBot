using System.Collections.Generic;

namespace SammBotNET.Classes
{
	public class AutoDequeueList<T> : LinkedList<T>
	{
		private readonly int MaxSize;

		public AutoDequeueList(int MaxSize) => this.MaxSize = MaxSize;

		public void Push(T Item)
		{
			this.AddFirst(Item);

			if (this.Count > MaxSize) this.RemoveLast();
		}
	}
}
