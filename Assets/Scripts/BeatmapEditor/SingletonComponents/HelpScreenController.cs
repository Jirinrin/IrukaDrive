using Shared.Domain;

namespace BeatmapEditor.SingletonComponents
{
    public class HelpScreenController : DataController<HelpScreenController>
    {
        private static Beatmap B => BeatmapEditorManager.currentBeatmap;

        private void Start() => Init(-1, false);

        public override void ToggleOpened(bool t)
        {
            base.ToggleOpened(t);
            if (opened)
            {
                if (SongDataController.Instance.opened)
                    SongDataController.Instance.ToggleOpened(false);
                if (ChartDataController.Instance.opened)
                    ChartDataController.Instance.ToggleOpened(false);
            }
        }
    }
}