using Shared.Domain;
using Tools.Commons;
using UnityEngine;

namespace Gameplay.SingletonComponents
{
    // todo: better 'shared container / coordinate' system to sync to EditorTrack
    [RequireComponent(typeof(SheetLineRecycler))]
    public class TrackSheetBg : Singleton<TrackSheetBg>
    {
        private SheetLineRecycler _sheetLines;

        public void InitSheet(Beatmap beatmap) => SheetLineRecycler.Instance.Init(beatmap);

        public void Refresh() => _sheetLines.RefreshWindow();

        private void OnEnable() => _sheetLines = GetComponent<SheetLineRecycler>();
    }
}