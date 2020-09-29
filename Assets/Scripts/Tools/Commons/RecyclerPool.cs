using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tools.Commons
{
    public class RecyclerPool<T> where T : MonoBehaviour
    {
        private readonly Func<T> _createItem;
    
        private readonly Stack<T> _items = new Stack<T>();
    
        public RecyclerPool(Func<T> createItem, int initialItemsNumber = 0)
        {
            _createItem = createItem;
            if (initialItemsNumber > 0)
                foreach (var i in Enumerable.Range(1, initialItemsNumber))
                    Add(_createItem());
        }

        public void Add(T item)
        {
            _items.Push(item);
        }

        public T Request()
        {
            return _items.Count > 0
                ? _items.Pop()
                : _createItem();
        }

        public void Destroy()
        {
            foreach (var item in _items)
                Object.Destroy(item.gameObject);
        }
    }
}