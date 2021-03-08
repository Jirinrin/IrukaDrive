using BeatmapEditor.Domain;
using Shared;
using Shared.Domain;
using Tools;
using UnityEngine;
using DG.Tweening;

namespace BeatmapEditor.SingletonComponents
{
    // todo: handle track not being initted yet
    public class EditorTrack : TrackBase<EditorTrack, EditorTrackViewState>
    {
        private EditorTrackNotesRecycler _notesRecycler;
        private EditorTrackSheetBg _sheet;
        private EditorTrackTimestamps _timestamps;
        private EditorWaveform _waveform;

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
            _timestamps = EditorTrackTimestamps.Instance;
            _waveform = EditorWaveform.Instance;
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

            _notesRecycler.Init();
            
            // todo: also refresh window?
            
            _sheet.InitSheet(beatmap.song);
            _sheet.DrawSheet(true); // todo: necessary?

            _timestamps.Init(beatmap.song);
            _timestamps.RefreshWindow(true);

            _waveform.LoadSong(beatmap.song);

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
                _timestamps.RefreshWindow(_zoomed);
                _zoomed = false;
            }
        }
        
        // Stuff responding to gestures

        public static float ScreenXToBeat(float screenX, float beatSnap = C.EditorBeatSnap) =>
            ((viewState.panX + screenX) / viewState.beatSpacing).RoundToNearest(beatSnap);

        public bool BeatIsVisible(float b) =>
            b > ScreenXToBeat(0) && b < ScreenXToBeat(containerRect.width);

        public void AddWord(BeatmapWord word)
        {
            BeatmapEditorManager.currentBeatmap.words.Add(word);
            BeatmapEditorManager.currentBeatmap.SortWords();
            RefreshBeatmap();
        }
        public void CreateWord(float screenXOrBeat, bool isBeat = false)
        {
            var beat = isBeat ? screenXOrBeat : ScreenXToBeat(screenXOrBeat);
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
            // Debug.Log($"pan by {deltaX}");
            viewState.Pan(deltaX);
            transform.localPosition = new Vector3(-viewState.panX, 0, 0);
            _shouldDraw = true;
        }

        public void PanToBeat(float beat, bool tweened = false)
        {
            // Debug.Log($"pan to beat: {beat}. visible from {ScreenXToBeat(0)} to {ScreenXToBeat(containerRect.width)}");
            if (BeatIsVisible(beat)) return;

            var beatXCentered = beat * viewState.beatSpacing - containerRect.width / 2f;
            if (tweened)
                DOTween.To(() => viewState.panX, PanToX, beatXCentered, .4f);
            else
                PanToX(beatXCentered);
        }

        private void PanToX(float x)
        {
            viewState.SetPan(x);
            transform.localPosition = new Vector3(-viewState.panX, 0, 0);
            _shouldDraw = true;
        }

        public void ResetTrack()
        {
            _notesRecycler.Cleanup();
            _sheet.Cleanup();
            _timestamps.Cleanup();
        }
    }
}
