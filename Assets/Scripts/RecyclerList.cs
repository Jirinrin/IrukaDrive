using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class RecyclerList<T> where T : MonoBehaviour
{    
    private readonly RecyclerPool<T> _recyclerPool;

    public readonly Dictionary<int, T> visibleItemsLookup = new Dictionary<int, T>();

    // todo: figure out best way to index this with binary tree
    private readonly SortedSet<int> _visibleItemIndices = new SortedSet<int>();

    // Must have 2 items
    private int[] _window;

    // (from, to) => newIndices
    private readonly Func<int, int, List<int>> _getNewItemIndicesInWindow;
    
    // (item, index) => void
    private readonly Action<T, int> _initItem;
    
    // (item, index) => void
    [CanBeNull] private readonly Action<T, int> _cleanupItem;

    public RecyclerList(
        Func<T> createItem, Action<T, int> initItem, Func<int, int, List<int>> getNewItemIndicesInWindow, int[] startWindow,
        [CanBeNull] Action<T, int> cleanupItem = null)
    {
        _initItem = initItem;
        _cleanupItem = cleanupItem;
        _recyclerPool = new RecyclerPool<T>(createItem);
        _getNewItemIndicesInWindow = getNewItemIndicesInWindow;
        _window = startWindow;
        AddToWindow(startWindow[0], startWindow[1]);
    }

    private void RemoveFromWindow(int from, int to)
    {
        // todo: check that GetViewBetween uses the right inclusive/exclusive thresholds
        var itemsToRemove = _visibleItemIndices.GetViewBetween(from, to).ToArray();
        // todo: iterate over sortedset without enumerating (? i.e. using the Count and for loop, is that quicker?)
        foreach (var itemIndex in itemsToRemove)
        {
            // todo: make more efficient?
            var item = visibleItemsLookup[itemIndex];
            item.gameObject.SetActive(false);
            _cleanupItem?.Invoke(item, itemIndex); // expensive?
            visibleItemsLookup.Remove(itemIndex);
            _visibleItemIndices.Remove(itemIndex);
            _recyclerPool.Add(item);
        }
    }
    
    private void AddToWindow(int from, int to)
    {
        var newIndices = _getNewItemIndicesInWindow(from, to);
        foreach (var itemIndex in newIndices)
        {
            // todo: make more efficient
            var item = _recyclerPool.Request();
            item.gameObject.SetActive(true);
            _initItem(item, itemIndex);
            visibleItemsLookup[itemIndex] = item;
            _visibleItemIndices.Add(itemIndex);
        }
    }

    public void SetVisibleWindow(int[] newWindow)
    {
        if (newWindow[0] > _window[1] || newWindow[1] < _window[0])
        {
            RemoveFromWindow(_window[0], _window[1]);
            AddToWindow(newWindow[0], newWindow[1]);
            _window = newWindow;
            return;
        }
        
        if (newWindow[0] > _window[0])
            RemoveFromWindow(_window[0], newWindow[0]);
        else if (newWindow[0] < _window[0])
            AddToWindow(newWindow[0], _window[0]);
        if (newWindow[1] > _window[1])
            AddToWindow(_window[1], newWindow[1]);
        else if (newWindow[1] < _window[1])
            RemoveFromWindow(newWindow[1], _window[1]);
        
        _window = newWindow;
    }

    public void Refresh()
    {
        RecycleAll();
        AddToWindow(_window[0], _window[1]);
    }

    private void RecycleAll() => RemoveFromWindow(_window[0], _window[1]);

    public void Destroy()
    {
        RecycleAll();
        _recyclerPool.Destroy();
    }
}