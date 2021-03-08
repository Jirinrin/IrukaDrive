using Shared;
using Shared.Domain;

namespace BeatmapEditor.SingletonComponents
{
    public class EditorSheetLineRecycler : SheetLineRecyclerBase
    {
        public override void Init(Song song)
        {
            containerRect = EditorTrack.Instance.containerRect;
            base.Init(song);
        }

        protected override float PanX => EditorTrack.viewState.panX;
        protected override float BeatSpacing => EditorTrack.viewState.beatSpacing;
    }
}