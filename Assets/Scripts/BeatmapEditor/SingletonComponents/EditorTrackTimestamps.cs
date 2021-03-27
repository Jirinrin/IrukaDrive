using System.Collections.Generic;
using Shared;
using Shared.Domain;
using TMPro;
using Tools;
using Tools.Commons;
using UnityEngine;

namespace BeatmapEditor.SingletonComponents
{
    public class EditorTrackTimestamps : Singleton<EditorTrackTimestamps>
    {
        [SerializeField] private TextMeshProUGUI timestampPrefab;
        private RecyclerList<TextMeshProUGUI> _recyclerList;

        private Song _currentSong;

        private TextMeshProUGUI CreateTimestamp() => Instantiate(timestampPrefab, transform);

        private void InitTimestamp(TextMeshProUGUI item, int index)
        {
            item.transform.localPosition = new Vector3(BeatSpacing * index.IndexToBeat(), 0, 0);
            var beatTime = Mathf.RoundToInt(index / C.BeatIndexFactor);
            var secTime = _currentSong.BeatsToSecs(beatTime);
            item.text = $"{Mathf.FloorToInt(secTime / 60f):D1}:{Mathf.FloorToInt(secTime % 60):D2}.{Mathf.FloorToInt((secTime % 1) * 10):D1}\n({beatTime})";
        }

        // コピペ from SheetLineRecyclerBase
        private List<int> GetNewIndicesInWindow(int from, int to)
        {
            var output = new List<int>();
            var startIndex = Mathf.CeilToInt(from / (C.BeatIndexFactor * _currentSong.beatsPerBar)) * C.BeatIndexFactorInt * _currentSong.beatsPerBar;
            for (var i = startIndex; i < to; i += C.BeatIndexFactorInt * _currentSong.beatsPerBar)
                output.Add(i);
            // Output will be empty if nothing was found in the interval
            return output;
        }

        // コピペ from SheetLineRecyclerBase
        private int[] GetCurrentWindow()
        {
            var containerWidthExtension = EditorTrack.Instance.containerRect.width * .1f;
            var minBeat = Mathf.Max((EditorTrack.viewState.panX - containerWidthExtension) / BeatSpacing, 0f);
            var maxBeat = minBeat + (EditorTrack.Instance.containerRect.width + containerWidthExtension*2f) / BeatSpacing;
            return new[] {minBeat.BeatToIndex(), maxBeat.BeatToIndex()};
        }

        public void Init(Song song)
        {
            _currentSong = song;
            _recyclerList = new RecyclerList<TextMeshProUGUI>(CreateTimestamp, InitTimestamp, GetNewIndicesInWindow, GetCurrentWindow());
        }

        public void RefreshWindow(bool updateSpacing = false)
        {
            _recyclerList.SetVisibleWindow(GetCurrentWindow());
            if (updateSpacing)
                foreach (var index in _recyclerList.visibleItemsLookup.Keys)
                    // todo: somehow make the accuracy smoother than 1/BeatIndexFactor again here
                    _recyclerList.visibleItemsLookup[index].transform.localPosition =
                        new Vector3(BeatSpacing * index.IndexToBeat(), 0, 0);
        }

        public void Cleanup() => _recyclerList.Destroy();

        private static float BeatSpacing => EditorTrack.viewState.beatSpacing;
    }
}