using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Components;
using Gameplay.Domain;
using Shared;
using TMPro;
using UnityEngine;

// todo: shared base with editor notes recycler
namespace Gameplay.SingletonComponents
{
    public class TrackNotesRecycler : TrackNotesRecyclerBase<TrackNotesRecycler, RuntimeWord, RuntimeNote, TextMeshProUGUI, RuntimeWordObject>
    {
        protected override void InitCharObj(TextMeshProUGUI charObj, RuntimeNote note)
        {
            charObj.text = note.character.ToString();
            charObj.color = Color.white;
        }

        protected override void InitWord(RuntimeWordObject item, int index)
        {
            base.InitWord(item, index);
        
            if (_wordAppearActionQueue.ContainsKey(index))
            {
                while (_wordAppearActionQueue[index].Any())
                    _wordAppearActionQueue[index].Dequeue().Invoke(item);
                _wordAppearActionQueue.Remove(index);
            }
        }

        private void LoadBeatmap(IEnumerable<RuntimeWord> words, float startBeatSpacing)
        {
            beatSpacing = startBeatSpacing;
            _wordAppearActionQueue = new Dictionary<int, Queue<Action<RuntimeWordObject>>>();
            base.LoadBeatmap(words);
        }

        public void Init(IEnumerable<RuntimeWord> words, float startBeatSpacing)
        {
            containerWidth = Track.Instance.containerRect.width;
            LoadBeatmap(words, startBeatSpacing);
            base.Init();
        }


        // This is for when you want to do something once a certain word appears
        private Dictionary<int, Queue<Action<RuntimeWordObject>>> _wordAppearActionQueue;
        public void EnqueueForWordAppear(int index, Action<RuntimeWordObject> action)
        {
            if (!_wordAppearActionQueue.ContainsKey(index))
                _wordAppearActionQueue[index] = new Queue<Action<RuntimeWordObject>>();
            _wordAppearActionQueue[index].Enqueue(action);
        }

        // Coming from TrackManager

        private void OnEnable()
        {
            Track.OnPan += OnPan;
        }
        private void OnDisable()
        {
            Track.OnPan -= OnPan;
        }
    }
}