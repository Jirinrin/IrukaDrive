using Shared.Domain;
using Tools.Commons;
using UnityEngine;

namespace BeatmapEditor.SingletonComponents
{
    [RequireComponent(typeof(EditorSheetLineRecycler))]
    public class EditorTrackSheetBg : Singleton<EditorTrackSheetBg>
    {
        private EditorSheetLineRecycler _sheetLines;

        public void InitSheet(Song song)
        {
            EditorSheetLineRecycler.Instance.Init(song);
        }

        public void DrawSheet(bool updateSpacing) => _sheetLines.RefreshWindow(updateSpacing);

        private void OnEnable() => _sheetLines = GetComponent<EditorSheetLineRecycler>();

        public void Cleanup() => _sheetLines.Cleanup();
    }
}