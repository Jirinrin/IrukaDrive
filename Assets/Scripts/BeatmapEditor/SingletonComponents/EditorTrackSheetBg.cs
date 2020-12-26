using Shared.Domain;
using Tools.Commons;
using UnityEngine;

namespace BeatmapEditor.SingletonComponents
{
    // todo: better 'shared container / coordinate' system to sync to EditorTrack
    [RequireComponent(typeof(EditorSheetLineRecycler))]
    public class EditorTrackSheetBg : Singleton<EditorTrackSheetBg>
    {
        private EditorSheetLineRecycler _sheetLines;

        public void InitSheet(Beatmap beatmap)
        {
            EditorSheetLineRecycler.Instance.Init(beatmap);
        }

        public void DrawSheet()
        {
            _sheetLines.RefreshWindow();
        }

        private void OnEnable()
        {
            _sheetLines = GetComponent<EditorSheetLineRecycler>();
        }
    }
}