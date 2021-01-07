using BeatmapEditor.Domain;
using Shared;
using Shared.Domain;
using Tools;
using UnityEngine;

namespace BeatmapEditor.SingletonComponents
{
    // todo: handle track not being initted yet
    public class EditorTrack : TrackBase<EditorTrack, EditorTrackViewState>
    {
        private EditorTrackNotesRecycler _notesRecycler;
        private EditorTrackSheetBg _sheet;

        private bool _shouldDraw;
        private bool _zoomed;

        private bool _initted;

        public void RefreshBeatmap()
        {
            _notesRecycler.RefreshBeatmap();
            _shouldDraw = true;
        }

        public void OnEnable()
        {
            _notesRecycler = EditorTrackNotesRecycler.Instance;
            _sheet = EditorTrackSheetBg.Instance;
        }

        protected override void Init()
        {
            base.Init();
            EditorTrackClipboard.Instance.Init();
            _initted = true;
        }

        public void LoadBeatmap(Beatmap beatmap, bool keepViewState = false)
        {
            if (keepViewState)
                viewState?.Init();
            else
                viewState = new EditorTrackViewState();
            
            if (!_initted)
                Init();

            _notesRecycler.Init(beatmap);
            
            // todo: also refresh window?
            
            _sheet.InitSheet(beatmap);
            _sheet.DrawSheet(true); // todo: necessary?

            Pan(0f);
            Zoom(0f, 0f);
        }

        private void Update()
        {
            if (_shouldDraw)
            {
                _shouldDraw = false;
                _notesRecycler.RefreshWindow(_zoomed);
                _sheet.DrawSheet(_zoomed);
                _zoomed = false;
            }
        }
        
        // Stuff responding to gestures

        public static float ScreenXToBeat(float screenX) => ((viewState.panX + screenX) / viewState.beatSpacing).RoundToNearest(C.EditorBeatSnap);

        public void AddWord(BeatmapWord word)
        {
            BeatmapEditorManager.currentBeatmap.words.Add(word);
            BeatmapEditorManager.currentBeatmap.SortWords();
            RefreshBeatmap();
        }
        public void CreateWord(float screenX)
        {
            var beat = ScreenXToBeat(screenX);
            AddWord(new BeatmapWord(beat));
            _notesRecycler.EditWord(beat.BeatToIndex());
        }

        public void PlayFromPoint(float screenX, bool autoplay) =>
            BeatmapEditorManager.Instance.PlayBeatmapFrom(Mathf.Max(ScreenXToBeat(screenX), 0f), autoplay);

        public void Zoom(float delta, float screenPivotX)
        {
            viewState.Zoom(delta, screenPivotX);
            transform.localPosition = new Vector3(-viewState.panX, 0, 0);
            _shouldDraw = true;
            _zoomed = true;
        }

        public void Pan(float deltaX)
        {
            viewState.Pan(deltaX);
            transform.localPosition = new Vector3(-viewState.panX, 0, 0);
            _shouldDraw = true;
        }

        public void ResetTrack()
        {
            _notesRecycler.Cleanup();
            _sheet.Cleanup();
        }
    }
}
