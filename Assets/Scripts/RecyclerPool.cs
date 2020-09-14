using System;
using System.Collections.Generic;
using UnityEngine;

public class RecyclerPool<T> where T : MonoBehaviour
{
    private readonly Func<T> _createItem;
    
    private readonly Stack<T> _items = new Stack<T>();
    
    public RecyclerPool(Func<T> createItem)
    {
        _createItem = createItem;
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
}