using System;
using Shared;
using Shared.Domain;

namespace Gameplay.Domain
{
    public class TrackViewState : ViewState
    {
        private readonly float _initBpm;

        public TrackViewState(float initBpm)
        {
            _initBpm = initBpm;
            
            SetBpm(initBpm);
            OnPan?.Invoke(panX);
            OnZoom?.Invoke(beatSpacing);
        }
        
        public void SetBpm(float bpm)
        {
            beatSpacing = (bpm / _initBpm) * C.DefaultScrollSpeed * Local.Settings.beatmapScrollSpeedMod;
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