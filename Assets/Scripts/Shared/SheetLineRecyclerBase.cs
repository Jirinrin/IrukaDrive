using System.Collections.Generic;
using Shapes;
using Shared.Domain;
using Tools.Commons;
using UnityEngine;

namespace Shared
{
    // todo: make sure the recycler thing gets updated not every frame
    // todo: better 'shared container / coordinate' system to sync to Track
    public class SheetLineRecyclerBase : Singleton<SheetLineRecyclerBase>
    {
        [SerializeField] protected Line barLinePrefab = null;
        [SerializeField] protected Line beatLinePrefab = null;

        private RecyclerList<Line> _recyclerList1;
        private RecyclerList<Line> _recyclerList2;

        private Beatmap _currentBeatmap;
        
        // Expected to be set from Init before calling base.Init
        protected Rect containerRect;

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
            var containerWidthExtension = containerRect.width * .1f;
            var minBeat = Mathf.Max((_panX - containerWidthExtension) / _beatSpacing, 0f);
            var maxBeat = minBeat + (containerRect.width + containerWidthExtension*2f) / _beatSpacing;
            return new[] {minBeat.BeatToIndex(), maxBeat.BeatToIndex()};
        }

        public virtual void Init(Beatmap beatmap)
        {
            var lineStartEndY = containerRect.height / 2f - 10f;
            var barLineTemplate = Instantiate(barLinePrefab);
            var beatLineTemplate = Instantiate(beatLinePrefab);
            barLineTemplate.Start = new Vector3(0, lineStartEndY, 0);
            barLineTemplate.End = new Vector3(0, -lineStartEndY, 0);
            beatLineTemplate.Start = new Vector3(0, lineStartEndY, 0);
            beatLineTemplate.End = new Vector3(0, -lineStartEndY, 0);
            barLinePrefab = barLineTemplate;
            beatLinePrefab = beatLineTemplate;
            
            _currentBeatmap = beatmap;

            _recyclerList1 = new RecyclerList<Line>(CreateBarLine, InitLine, GetNewLineIndicesInWindowBar, GetCurrentWindow());
            _recyclerList2 = new RecyclerList<Line>(CreateBeatLine, InitLine, GetNewLineIndicesInWindowBeat, GetCurrentWindow());
        }

        public void RefreshWindow(bool updateSpacing = false)
        {
            _recyclerList1.SetVisibleWindow(GetCurrentWindow());
            _recyclerList2.SetVisibleWindow(GetCurrentWindow());
            if (updateSpacing)
            {
                UpdateLinesSpacing(_recyclerList1);
                UpdateLinesSpacing(_recyclerList2);
            }
        }

        public void Cleanup()
        {
            _recyclerList1.Destroy();
            _recyclerList2.Destroy();
        }

        // Coming from Track
        
        private float _panX;
        private float _beatSpacing;
        protected void OnPan(float panX) => _panX = panX;
        protected void OnZoom(float beatSpacing) => _beatSpacing = beatSpacing;
    }
}