using System;
using Shared.Domain;
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
        

        public EditorTrackViewState() => Init();

        public void Init()
        {
            OnPan?.Invoke();
            OnZoom?.Invoke();
        }

        public void Pan(float deltaX)
        {
            panX = Mathf.Max(panX - deltaX, MinimumPan);
            OnPan?.Invoke();
        }

        public void SetPan(float x)
        {
            panX = Mathf.Max(x, MinimumPan);
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