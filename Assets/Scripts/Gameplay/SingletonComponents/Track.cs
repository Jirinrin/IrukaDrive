using System.Collections.Generic;
using System.Linq;
using Gameplay.Components;
using Gameplay.Domain;
using Shared;
using Shared.Domain;
using UnityEngine;

namespace Gameplay.SingletonComponents
{
    public class Track : TrackBase<Track, TrackViewState>
    {
        [SerializeField] public RectTransform judgementPoint;
    
        private TrackNotesRecycler _notesRecycler;
        private TrackSheetBg _sheet;

        private float _judgementOffsetX;
    
        private bool _shouldDraw;

        private RuntimeWordObject _currentWordObj;

        public void InitTrack(Beatmap beatmap, IEnumerable<RuntimeWord> words)
        {
            viewState = new TrackViewState(beatmap.bpm);
        
            InitContainerRect();

            _notesRecycler.Init(words);
            _sheet.InitSheet(beatmap);
        
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
                ForceRefresh();
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
                SetCurrentWordObj(_notesRecycler.VisibleWordObjects[index].obj);
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
                _currentWordObj.charObjRefs.Last().obj.color = Color.white;
                return;
            }

            var index = (int) charIndex;
            
            if (index >= _currentWordObj.charObjRefs.Count)
                return;
        
            _currentWordObj.charObjRefs[index].obj.color = Color.red;
            if (index > 0)
                _currentWordObj.charObjRefs[index-1].obj.color = Color.white;
        }

        public void ForceRefresh()
        {
            _notesRecycler.RefreshWindow();
            _sheet.Refresh();
        }

        private int _updateProgressCounter;
        public void UpdateProgress(float posBeats)
        {
            viewState.SetProgress(posBeats);
            transform.localPosition = new Vector3(_judgementOffsetX - viewState.panX, 0, 0);
            
            // Crappy mechanism to not refresh window on every loop
            if ((_updateProgressCounter = (_updateProgressCounter + 1) % 10) == 0) 
                _shouldDraw = true;
        }

        private void OnEnable()
        {
            _notesRecycler = TrackNotesRecycler.Instance;
            _sheet = TrackSheetBg.Instance;
            
            GameplayManager.OnChangeCurrentWord += ChangeCurrentWord;
            GameplayManager.OnChangeCurrentChar += ChangeCurrentChar;
        }

        private void OnDisable()
        {
            GameplayManager.OnChangeCurrentWord -= ChangeCurrentWord;
            GameplayManager.OnChangeCurrentChar -= ChangeCurrentChar;
        }
    }
}
