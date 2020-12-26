using System;
using Shared;

namespace Gameplay.Domain
{
    public class TrackViewState
    {
        public float panX;

        private float _scaleX = 1f;
        public float beatSpacing;

        public float initBpm;

        public TrackViewState(float initBpm)
        {
            this.initBpm = initBpm;
            
            SetBpm(initBpm);
            OnPan?.Invoke(panX);
            OnZoom?.Invoke(beatSpacing);
        }
        
        public void SetBpm(float bpm)
        {
            beatSpacing = (bpm / initBpm) * C.DefaultScrollSpeed * Local.Settings.beatmapScrollSpeedMod;
            // todo: maybe still need to do some panning like in EditorTrackViewState?
            OnZoom?.Invoke(beatSpacing);
        }

        public void SetProgress(float posBeats)
        {
            panX = posBeats * beatSpacing;
            OnPan?.Invoke(panX);
        }

        public static event Action<float> OnPan;
        public static event Action<float> OnZoom;
    }
}