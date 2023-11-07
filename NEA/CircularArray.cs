using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEA
{
    internal class CircularArray<T>
    {
        private T[] Data;
        private int StartIndex;
        private int EndIndex;
        private int MaxSize;
        public int CurrentSize { get; private set; }
        public CircularArray(int maxSize)
        {
            MaxSize = maxSize;
            Data = new T[MaxSize];
            StartIndex = 0;
            EndIndex = -1;
            CurrentSize = 0;
        }
        public void AddItem(T item)
        {
            EndIndex = (EndIndex + 1) % MaxSize;
            CurrentSize++;
            if (CurrentSize > MaxSize)
            {
                RemoveItemAtStart();
            }
            Data[EndIndex] = item;
        }
        public void RemoveItemAtStart()
        {
            if (CurrentSize > 0)
            {
                CurrentSize--;
                StartIndex = (StartIndex + 1) % MaxSize;
            }
        }
        public T this[int i]
        {
            get
            {
                if (i < CurrentSize)
                {
                    int index = (StartIndex + i) % MaxSize;
                    return Data[index];
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }
    }
}
