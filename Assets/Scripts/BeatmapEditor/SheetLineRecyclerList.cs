using System.Collections.Generic;
using Shapes;
using UnityEngine;

namespace BeatmapEditor
{
    // todo: make sure the recycler thing gets updated not every frame
    // todo: better 'shared container / coordinate' system to sync to EditorTrack
    // todo: make shared thing with 'normal' sheetlinerecyclerlist
    public class SheetLineRecyclerList : Singleton<SheetLineRecyclerList>
    {
        [SerializeField] private Line barLinePrefab = null;
        [SerializeField] private Line beatLinePrefab = null;

        private RecyclerList<Line> _recyclerList1;
        private RecyclerList<Line> _recyclerList2;

        private Beatmap _currentBeatmap;

        private Line CreateBarLine() => Instantiate(barLinePrefab, transform);
        private Line CreateBeatLine() => Instantiate(beatLinePrefab, transform);

        private void InitLine(Line item, int index) =>
            item.transform.localPosition = new Vector3(_beatSpacing * index.IndexToBeat(), 0, 0);

        private void UpdateLinesSpacing(RecyclerList<Line> list)
        {
            foreach (var index in list.visibleItemsLookup.Keys)
            {
                // todo: somehow make the accuracy smoother than 1/BeatIndexFactor again here
                list.visibleItemsLookup[index].transform.localPosition = new Vector3(_beatSpacing * index.IndexToBeat(), 0, 0);
            }
        }

        private List<int> GetNewLineIndicesInWindow(int from, int to, bool isBar)
        {
            var output = new List<int>();
            var startIndex = Mathf.CeilToInt(from / C.BeatIndexFactor) * C.BeatIndexFactorInt;
            for (var i = startIndex; i < to; i += C.BeatIndexFactorInt)
            {
                var onBar = i % (_currentBeatmap.beatsPerBar * C.BeatIndexFactorInt) == _currentBeatmap.barOffset * C.BeatIndexFactorInt;
                if (isBar && onBar || !isBar && !onBar)
                    output.Add(i);
            }
            // Output will be empty if nothing was found in the interval
            return output;
        }
        private List<int> GetNewLineIndicesInWindowBar(int from, int to) => GetNewLineIndicesInWindow(from, to, true);
        private List<int> GetNewLineIndicesInWindowBeat(int from, int to) => GetNewLineIndicesInWindow(from, to, false);

        private int[] GetCurrentWindow()
        {
            var minBeat = Mathf.Max(_panX / _beatSpacing, 0f);
            var maxBeat = minBeat + EditorTrack.Instance.containerRect.width / _beatSpacing;
            return new[] {minBeat.BeatToIndex(), maxBeat.BeatToIndex()};
        }
        
        public void Init(Beatmap beatmap)
        {
            var lineStartEndY = EditorTrack.Instance.containerRect.height / 2f - 10f;
            barLinePrefab.Start = new Vector3(0, lineStartEndY, 0);
            barLinePrefab.End = new Vector3(0, -lineStartEndY, 0);
            beatLinePrefab.Start = new Vector3(0, lineStartEndY, 0);
            beatLinePrefab.End = new Vector3(0, -lineStartEndY, 0);
            
            _currentBeatmap = beatmap;
            
            _recyclerList1 = new RecyclerList<Line>(CreateBarLine, InitLine, GetNewLineIndicesInWindowBar, GetCurrentWindow());
            _recyclerList2 = new RecyclerList<Line>(CreateBeatLine, InitLine, GetNewLineIndicesInWindowBeat, GetCurrentWindow());
        }

        public void RefreshWindow()
        {
            _recyclerList1.SetVisibleWindow(GetCurrentWindow());
            _recyclerList2.SetVisibleWindow(GetCurrentWindow());
            UpdateLinesSpacing(_recyclerList1);
            UpdateLinesSpacing(_recyclerList2);
        }

        // Coming from EditorTrack
        
        private float _panX;
        private float _beatSpacing;
        private void OnPan(float panX) => _panX = panX;
        private void OnZoom(float beatSpacing) => _beatSpacing = beatSpacing;
        private void OnEnable()
        {
            EditorTrack.OnPan += OnPan;
            EditorTrack.OnZoom += OnZoom;
        }
        private void OnDisable()
        {
            EditorTrack.OnPan -= OnPan;
            EditorTrack.OnZoom -= OnZoom;
        }
    }
}