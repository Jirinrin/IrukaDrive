using Gameplay.Domain;
using Shared;
using Shared.Domain;

namespace Gameplay.SingletonComponents
{
    public class SheetLineRecycler : SheetLineRecyclerBase
    {
        public override void Init(Beatmap beatmap)
        {
            containerRect = Track.Instance.containerRect;
            base.Init(beatmap);
        }
        
        private void OnEnable()
        {
            TrackViewState.OnPan += OnPan;
            TrackViewState.OnZoom += OnZoom;
        }
        private void OnDisable()
        {
            TrackViewState.OnPan -= OnPan;
            TrackViewState.OnZoom -= OnZoom;
        }
    }
}