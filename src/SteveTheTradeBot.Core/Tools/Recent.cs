using System.Collections.Generic;

namespace SteveTheTradeBot.Core.Tools
{
    public class Recent<T> : List<T>
    {
        private readonly int _size;

        public Recent(int size) : base(size)
        {
            _size = size;
        }

        public void Push(T candle)
        {
            if (Count >= _size)
            {
                RemoveAt(0);
            }

            Add(candle);
        }
    }
}