using System;
using UnityEngine;

namespace BeatmapEditor.Domain
{
    public class EditorTrackViewState
    {
        private const float MinimumPan = -20f;
        private const float DefaultBeatSpacing = 20f;
        private const float MaxZoomScale = 5f;
        
        // panX gets inverted to make 'the pan amount' more intuitive to deal with
        public float panX = MinimumPan;

        private float _scaleX = 1f;
        public float beatSpacing = DefaultBeatSpacing;
        

        public EditorTrackViewState() => Init();

        public void Init()
        {
            OnPan?.Invoke(panX);
            OnZoom?.Invoke(beatSpacing);
        }

        public void Pan(float deltaX)
        {
            panX = Mathf.Max(panX - deltaX, MinimumPan);
            OnPan?.Invoke(panX);
        }
        
        public void Zoom(float delta, float screenPivotX)
        {
            var oldScale = _scaleX;
            _scaleX = Mathf.Clamp(_scaleX + delta, 1f, MaxZoomScale);
            beatSpacing = _scaleX * DefaultBeatSpacing;
            var scaleDiff = _scaleX / oldScale;
            
            var pivotX = panX + screenPivotX;
            Pan(-pivotX * (scaleDiff-1f));
            OnZoom?.Invoke(beatSpacing);
        }

        public static event Action<float> OnPan;
        public static event Action<float> OnZoom;
    }
}