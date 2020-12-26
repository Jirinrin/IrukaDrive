using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Components;
using Gameplay.Domain;
using Shared;
using Shared.Domain;
using Tools.Commons;
using UnityEngine;

namespace Gameplay.SingletonComponents
{
    // todo: effect when player hits / misses / nears char, and make that persist in the char as it moves beyond the hantei
    public class Track : Singleton<Track>
    {
        [SerializeField] private RectTransform containerRectTransform = null;
        [NonSerialized] public Rect containerRect;

        [SerializeField] public RectTransform judgementPoint;
    
        private TrackNotesRecycler _notesRecycler;
        // todo: add sheet
        // private EditorTrackSheetBg _sheet;

        private float _judgementOffsetX;
    
        private bool _shouldDraw;
        
        private RuntimeWordObject _currentWordObj;
        public void InitTrack(Beatmap beatmap, IEnumerable<RuntimeWord> words)
        {
            containerRect = containerRectTransform.rect;
        
            Debug.Log("init track");

            InitBpm(beatmap.bpm);
            OnPan?.Invoke(_panX);
        
            _notesRecycler = TrackNotesRecycler.Instance;
            _notesRecycler.Init(words, _beatSpacing);
        
            // _sheet = EditorTrackSheetBg.Instance;
            // _sheet.InitSheet(beatmap);
        
            UpdateProgress(0f);
        }

        private void Start()
        {
            _judgementOffsetX = judgementPoint.localPosition.x;
        }

        private void Update()
        {
            if (_shouldDraw)
            {
                _shouldDraw = false;
                // todo: only do this every so often
                _notesRecycler.RefreshWindow();
                // _sheet.DrawSheet();
            }
        }

        private void SetCurrentWordObj(RuntimeWordObject obj)
        {
            _currentWordObj = obj;
            ChangeCurrentChar(0);
        }

        private void ChangeCurrentWord(float beat)
        {
            // todo: de-mark previous current word if there was still some current
            var index = beat.BeatToIndex();
            if (_notesRecycler.VisibleWordObjects.ContainsKey(index))
                SetCurrentWordObj(_notesRecycler.VisibleWordObjects[index]);
            else
                _notesRecycler.EnqueueForWordAppear(index, SetCurrentWordObj);
            // If they're transparent by default:
            // foreach (var obj in _currentWordObjects)
            //     obj.color = Color.white;
        }
    
        private void ChangeCurrentChar(int? charIndex)
        {
            if (_currentWordObj.charObjRefs == null)
                return;
            
            if (charIndex == null)
            {
                _currentWordObj.charObjRefs.Last().color = Color.white;
                return;
            }

            var index = (int) charIndex;
            
            if (index >= _currentWordObj.charObjRefs.Count)
                return;
        
            _currentWordObj.charObjRefs[index].color = Color.red;
            if (index > 0)
                _currentWordObj.charObjRefs[index-1].color = Color.white;
        }

        public void ForceRefresh() => _notesRecycler.RefreshWindow();
    
        private float _panX;

        public void UpdateProgress(float posBeats)
        {
            _panX = posBeats * _beatSpacing;
            transform.localPosition = new Vector3(_judgementOffsetX - _panX, 0, 0);
            OnPan?.Invoke(_panX);
            _shouldDraw = true;
        }

        private float _initBpm;
        private void InitBpm(float bpm)
        {
            _initBpm = bpm;
            SetBpm(_initBpm);
        }
    
        private float _beatSpacing;
        public void SetBpm(float bpm) =>
            _beatSpacing = (bpm / _initBpm) * C.DefaultScrollSpeed * Local.Settings.beatmapScrollSpeedMod;

        private void OnEnable()
        {
            GameplayManager.OnChangeCurrentWord += ChangeCurrentWord;
            GameplayManager.OnChangeCurrentChar += ChangeCurrentChar;
        }
    
        public static event Action<float> OnPan;
    }
}
