using System;
using Shared;
using Shared.Domain;
using Tools;
using Tools.Commons;
using UnityEngine;

namespace BeatmapEditor.SingletonComponents
{
    // todo: better 'shared container / coordinate' system to sync to EditorTrack
    // todo: handle track not being initted yet
    public class EditorTrack : Singleton<EditorTrack>
    {
        [SerializeField] private RectTransform containerRectTransform = null;
        [NonSerialized] public Rect containerRect;

        private EditorTrackNotesRecycler _notesRecycler;
        private EditorTrackSheetBg _sheet;

        private bool _shouldDraw;

        public void RefreshBeatmap()
        {
            _notesRecycler.RefreshBeatmap();
            _shouldDraw = true;
        }

        public void InitTrack(Beatmap beatmap)
        {
            containerRect = containerRectTransform.rect;
            
            Debug.Log("init track");

            OnPan?.Invoke(_panX);
            OnZoom?.Invoke(_beatSpacing);

            _notesRecycler = EditorTrackNotesRecycler.Instance;
            _notesRecycler.Init(beatmap);
            // todo: also refresh window?
            
            _sheet = EditorTrackSheetBg.Instance;
            _sheet.InitSheet(beatmap);
            
            _sheet.DrawSheet(); // necessary?
            
            Pan(0f);
            Zoom(0f, 0f);
        }

        private void Update()
        {
            if (_shouldDraw)
            {
                _shouldDraw = false;
                _notesRecycler.RefreshWindow();
                _sheet.DrawSheet();
            }
        }
        
        // Stuff responding to gestures

        private float ScreenXToBeat(float screenX) => ((_panX + screenX) / _beatSpacing).RoundToNearest(C.EditorBeatSnap); 

        public void CreateWord(float screenX)
        {
            var newWordBeat = ScreenXToBeat(screenX);
            var newWord = new BeatmapWord(newWordBeat);
            BeatmapEditorManager.Instance.currentBeatmap.words.Add(newWord);
            RefreshBeatmap();
            _notesRecycler.EditWord(newWordBeat.BeatToIndex());
        }

        public void PlayFromPoint(float screenX) =>
            BeatmapEditorManager.Instance.PlayBeatmapFrom(ScreenXToBeat(screenX));

        private const float DefaultBeatSpacing = 20f;
        private const float MaxZoomScale = 5f;
        private float _scaleX = 1f;
        private float _beatSpacing = DefaultBeatSpacing;
        public void Zoom(float delta, float screenPivotX)
        {
            var oldScale = _scaleX;
            _scaleX = Mathf.Clamp(_scaleX + delta, 1f, MaxZoomScale);
            _beatSpacing = _scaleX * DefaultBeatSpacing;
            var scaleDiff = _scaleX / oldScale;
            
            var pivotX = _panX + screenPivotX;
            Pan(-pivotX * (scaleDiff-1f));
            _shouldDraw = true;
            OnZoom?.Invoke(_beatSpacing);
        }

        // panX gets inversed to make 'the pan amount' more intuitive to deal with
        private const float MinimumPan = -20f;
        private float _panX = MinimumPan;
        public void Pan(float deltaX)
        {
            _panX = Mathf.Max(_panX - deltaX, MinimumPan);
            transform.localPosition = new Vector3(-_panX, 0, 0);
            OnPan?.Invoke(_panX);
            _shouldDraw = true;
        }

        public static event Action<float> OnPan;
        public static event Action<float> OnZoom;
    }
}
