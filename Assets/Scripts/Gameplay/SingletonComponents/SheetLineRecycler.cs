using Gameplay.Domain;
using Shared;
using Shared.Domain;

namespace Gameplay.SingletonComponents
{
    public class SheetLineRecycler : SheetLineRecyclerBase
    {
        public override void Init(Song song)
        {
            containerRect = Track.Instance.containerRect;
            base.Init(song);
        }

        protected override float PanX => Track.viewState.panX;
        protected override float BeatSpacing => Track.viewState.beatSpacing;
    }
}