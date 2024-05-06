using System;

namespace Script.Utility
{
    public class Heap<T> where T : IHeapItem<T>
    {
        private readonly T[] _items;
        private int _currentItemCount;
    

        public int Count => _currentItemCount;

        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        public Heap(int maxHeapSize)
        {
            _items = new T[maxHeapSize];
        }

        public void Add(T item)
        {
            if (_currentItemCount == _items.Length)
                return;
            item.HeapIndex = _currentItemCount;
            _items[_currentItemCount] = item;
            SortUp(item);
            _currentItemCount++;
        }

        public T RemoveFirst()
        {
            if (_currentItemCount == 0)
                return default(T);

            T firstItem = _items[0];
            _currentItemCount--;

            T lastItem = _items[_currentItemCount];
            _items[0] = lastItem;
            lastItem.HeapIndex = 0;

            SortDown(_items[0]);
            return firstItem;
        }

        private void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;
            while (parentIndex >= 0 && item.CompareTo(_items[parentIndex]) > 0)
            {
                T parentItem = _items[parentIndex];
                Swap(item, parentItem);

                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        private void SortDown(T item)
        {
            while (true)
            {
                int leftChildIndex = item.HeapIndex * 2 + 1;
                int rightChildIndex = item.HeapIndex * 2 + 2;

                if (leftChildIndex < _currentItemCount)
                {
                    int swapIndex = leftChildIndex;
                    if (rightChildIndex < _currentItemCount &&
                        _items[leftChildIndex].CompareTo(_items[rightChildIndex]) < 0)
                        swapIndex = rightChildIndex;

                    if (item.CompareTo(_items[swapIndex]) < 0)
                        Swap(item, _items[swapIndex]);
                    else
                        break;
                }
                else
                    break;
            }
        }

        private void Swap(T firstItem, T secondItem)
        {
            _items[secondItem.HeapIndex] = firstItem;
            _items[firstItem.HeapIndex] = secondItem;

            (firstItem.HeapIndex, secondItem.HeapIndex) = (secondItem.HeapIndex, firstItem.HeapIndex);
        }

        public bool Contains(T item)
        {
            if (item.HeapIndex < _currentItemCount)
                return Equals(_items[item.HeapIndex], item);
            else
                return false;
        }

        public void Clear()
        {
            _currentItemCount = 0;
        }
    }

    public interface IHeapItem<in T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }
}