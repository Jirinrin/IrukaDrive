using Shared.Domain;
using Tools.Commons;
using UnityEngine;

namespace Gameplay.SingletonComponents
{
    [RequireComponent(typeof(SheetLineRecycler))]
    public class TrackSheetBg : Singleton<TrackSheetBg>
    {
        private SheetLineRecycler _sheetLines;

        public void InitSheet(Song song) => SheetLineRecycler.Instance.Init(song);

        public void Refresh() => _sheetLines.RefreshWindow();

        private void OnEnable() => _sheetLines = GetComponent<SheetLineRecycler>();
    }
}