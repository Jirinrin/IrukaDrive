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

        protected override float PanX => Track.viewState.panX;
        protected override float BeatSpacing => Track.viewState.beatSpacing;
    }
}