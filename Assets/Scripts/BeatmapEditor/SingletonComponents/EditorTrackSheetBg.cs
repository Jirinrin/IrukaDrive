using Shared.Domain;
using Tools.Commons;
using UnityEngine;

namespace BeatmapEditor.SingletonComponents
{
    // todo: better 'shared container / coordinate' system to sync to EditorTrack
    [RequireComponent(typeof(SheetLineRecyclerList))]
    public class EditorTrackSheetBg : Singleton<EditorTrackSheetBg>
    {
        private SheetLineRecyclerList _sheetLines;

        private float _panX;
        private float _beatSpacing;

        public void InitSheet(Beatmap beatmap)
        {
            SheetLineRecyclerList.Instance.Init(beatmap);
        }

        public void DrawSheet()
        {
            _sheetLines.RefreshWindow();
        }

        private void OnEnable()
        {
            _sheetLines = GetComponent<SheetLineRecyclerList>();
        }
        private void OnDisable()
        {
        }
    }
}