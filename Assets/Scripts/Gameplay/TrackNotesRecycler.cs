using System;
using System.Collections.Generic;
using System.Linq;
using Shared;
using TMPro;
using Tools.Commons;
using UnityEngine;

// todo: shared base with editor notes recycler
namespace Gameplay
{
    public class TrackNotesRecycler : TrackNotesRecyclerBase<TrackNotesRecycler, RuntimeWord, RuntimeNote, TextMeshProUGUI, RuntimeWordObject>
    {
        protected override void InitCharObj(TextMeshProUGUI charObj, RuntimeNote note)
        {
            charObj.text = note.Char.ToString();
            charObj.color = Color.white;
        }

        protected override void InitWord(RuntimeWordObject item, int index)
        {
            base.InitWord(item, index);
        
            if (_wordAppearActionQueue.ContainsKey(index))
            {
                while (_wordAppearActionQueue[index].Any())
                    _wordAppearActionQueue[index].Dequeue().Invoke();
                _wordAppearActionQueue.Remove(index);
            }
        }

        private void LoadBeatmap(List<RuntimeWord> words, float startBeatSpacing)
        {
            _beatSpacing = startBeatSpacing;
            _wordAppearActionQueue = new Dictionary<int, Queue<Action>>();
            base.LoadBeatmap(words);
        }

        public void Init(List<RuntimeWord> words, float startBeatSpacing)
        {
            containerWidth = TrackManager.Instance.containerRect.width;
            LoadBeatmap(words, startBeatSpacing);
            base.Init();
        }


        // This is for when you want to do something once a certain word appears
        private Dictionary<int, Queue<Action>> _wordAppearActionQueue;
        public void EnqueueForWordAppear(int index, Action action)
        {
            if (!_wordAppearActionQueue.ContainsKey(index))
                _wordAppearActionQueue[index] = new Queue<Action>();
            _wordAppearActionQueue[index].Enqueue(action);
        }

        // Coming from TrackManager

        private void OnEnable()
        {
            TrackManager.OnPan += OnPan;
        }
        private void OnDisable()
        {
            TrackManager.OnPan -= OnPan;
        }
    }
}