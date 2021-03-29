using System;
using Shared.Domain;
using Tools;
using UnityEngine;

namespace BeatmapEditor.Domain
{
    public class EditorTrackViewState : ViewState
    {
        private const float MinimumPan = -20f;
        private const float DefaultBeatSpacing = 20f;
        private const float MaxZoomScale = 5f;
        
        public new float panX = MinimumPan;
        public new float beatSpacing = DefaultBeatSpacing;

        private float _maxPanBeat = 10000f;
        private float _screenWidth;

        public async void Init(Song song, float screenWidth)
        {
            OnPan?.Invoke();
            OnZoom?.Invoke();
            _maxPanBeat = 10000f;
            _maxPanBeat = song.SecToBeats((await song.Audio)?.length ?? 10000f);
            _screenWidth = screenWidth;
        }

        public void Pan(float deltaX) => SetPan(panX - deltaX);

        public void SetPan(float x)
        {
            panX = Mathf.Clamp(x, MinimumPan, _maxPanBeat*beatSpacing - _screenWidth + 100f);
            OnPan?.Invoke();
        }
        
        public void Zoom(float delta, float screenPivotX)
        {
            var oldScale = scaleX;
            scaleX = Mathf.Clamp(scaleX + delta, 1f, MaxZoomScale);
            beatSpacing = scaleX * DefaultBeatSpacing;
            var scaleDiff = scaleX / oldScale;
            
            var pivotX = panX + screenPivotX;
            Pan(-pivotX * (scaleDiff-1f));
            OnZoom?.Invoke();
        }

        public static event Action OnPan;
        public static event Action OnZoom;
    }
}