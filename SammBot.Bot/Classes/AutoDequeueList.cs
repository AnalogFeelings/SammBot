using System.Collections.Generic;

namespace SammBot.Bot.Classes
{
    public class AutoDequeueList<T> : LinkedList<T>
    {
        private readonly int _MaxSize;

        public AutoDequeueList(int MaxSize) => this._MaxSize = MaxSize;

        public void Push(T Item)
        {
            this.AddFirst(Item);

            if (this.Count > _MaxSize) this.RemoveLast();
        }
    }
}
