using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

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

    public void Destroy()
    {
        foreach (var item in _items)
            Object.Destroy(item.gameObject);
    }
}