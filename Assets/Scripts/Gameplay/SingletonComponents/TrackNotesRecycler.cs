using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Components;
using Gameplay.Domain;
using Shared;
using Shared.Domain;
using UnityEngine;

namespace Gameplay.SingletonComponents
{
    public class TrackNotesRecycler : TrackNotesRecyclerBase<TrackNotesRecycler, RuntimeWord, RuntimeChar, RuntimeWordObject>
    {
        protected override void InitCharObj(CharObject charObj, RuntimeChar ch)
        {
            base.InitCharObj(charObj, ch);
            charObj.obj.color = Color.white;
        }

        protected override void InitWord(ObjWidthItem item)
        {
            base.InitWord(item);
        
            if (_wordAppearActionQueue.ContainsKey(item.startIndex))
            {
                while (_wordAppearActionQueue[item.startIndex].Any())
                    _wordAppearActionQueue[item.startIndex].Dequeue().Invoke(item.obj);
                _wordAppearActionQueue.Remove(item.startIndex);
            }
        }

        private new void LoadBeatmap(IEnumerable<RuntimeWord> words)
        {
            _wordAppearActionQueue = new Dictionary<int, Queue<Action<RuntimeWordObject>>>();
            base.LoadBeatmap(words);
        }

        public void Init(IEnumerable<RuntimeWord> words)
        {
            containerWidth = Track.Instance.containerRect.width;
            LoadBeatmap(words);
        }


        // This is for when you want to do something once a certain word appears
        private Dictionary<int, Queue<Action<RuntimeWordObject>>> _wordAppearActionQueue;
        public void EnqueueForWordAppear(int index, Action<RuntimeWordObject> action)
        {
            if (!_wordAppearActionQueue.ContainsKey(index))
                _wordAppearActionQueue[index] = new Queue<Action<RuntimeWordObject>>();
            _wordAppearActionQueue[index].Enqueue(action);
        }

        protected override float PanX => Track.viewState.panX;
        protected override float BeatSpacing => Track.viewState.beatSpacing;
    }
}