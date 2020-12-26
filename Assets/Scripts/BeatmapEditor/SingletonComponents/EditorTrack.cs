using System;
using BeatmapEditor.Domain;
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

        private static EditorTrackViewState _viewState;

        public void RefreshBeatmap()
        {
            _notesRecycler.RefreshBeatmap();
            _shouldDraw = true;
        }

        public void InitTrack(Beatmap beatmap, bool keepViewState = false)
        {
            containerRect = containerRectTransform.rect;

            if (keepViewState)
                _viewState?.Init();
            else
                _viewState = new EditorTrackViewState();

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

        private float ScreenXToBeat(float screenX) => ((_viewState.panX + screenX) / _viewState.beatSpacing).RoundToNearest(C.EditorBeatSnap); 

        public void CreateWord(float screenX)
        {
            var newWordBeat = ScreenXToBeat(screenX);
            var newWord = new BeatmapWord(newWordBeat);
            BeatmapEditorManager.currentBeatmap.words.Add(newWord);
            RefreshBeatmap();
            _notesRecycler.EditWord(newWordBeat.BeatToIndex());
        }

        public void PlayFromPoint(float screenX, bool autoplay) =>
            BeatmapEditorManager.Instance.PlayBeatmapFrom(ScreenXToBeat(screenX), autoplay);

        public void Zoom(float delta, float screenPivotX)
        {
            _viewState.Zoom(delta, screenPivotX);
            transform.localPosition = new Vector3(-_viewState.panX, 0, 0);
            _shouldDraw = true;
        }

        public void Pan(float deltaX)
        {
            _viewState.Pan(deltaX);
            transform.localPosition = new Vector3(-_viewState.panX, 0, 0);
            _shouldDraw = true;
        }
    }
}
