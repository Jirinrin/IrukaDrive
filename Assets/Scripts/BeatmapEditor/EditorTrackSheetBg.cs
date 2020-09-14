using System.Collections.Generic;
using Shapes;
using TMPro;
using UnityEngine;

namespace BeatmapEditor
{
    // todo: better 'shared container / coordinate' system to sync to EditorTrack
    public class EditorTrackSheetBg : Singleton<EditorTrackSheetBg>
    {
        [SerializeField] private Line barLinePrefab;
        [SerializeField] private Line beatLinePrefab;
        
        private Dictionary<int, Line> _sheetLineObjectLookup;

        private Rect _containerRect;
        private Beatmap _beatmap;
        
        public void InitSheet(Rect containerRect, Beatmap beatmap)
        {
            _sheetLineObjectLookup = new Dictionary<int, Line>();

            _beatmap = beatmap;
            _containerRect = containerRect;
            
            var lineStartEndY = _containerRect.height / 2f - 10f;
            barLinePrefab.Start = new Vector3(0, lineStartEndY, 0);
            barLinePrefab.End = new Vector3(0, -lineStartEndY, 0);
            beatLinePrefab.Start = new Vector3(0, lineStartEndY, 0);
            beatLinePrefab.End = new Vector3(0, -lineStartEndY, 0);
        }

        public void DrawSheet(float panX, float beatSpacing)
        {
            DrawSheetLines(panX, beatSpacing);
        }
        
        private void DrawSheetLines(float panX, float beatSpacing)
        {
            var minBeat = Mathf.Max(panX / beatSpacing, 0f);
            var maxBeat = minBeat + _containerRect.width / beatSpacing;

            for (var i = Mathf.FloorToInt(minBeat); i < Mathf.CeilToInt(maxBeat); i++)
            {
                // todo: recycle stuff
                if (!_sheetLineObjectLookup.ContainsKey(i))
                {
                    var linePrefab = i % _beatmap.beatsPerBar == _beatmap.barOffset ? barLinePrefab : beatLinePrefab;
                    _sheetLineObjectLookup[i] = Instantiate(linePrefab, transform);
                    // _sheetLineObjectLookup[i] = Instantiate(beatLinePrefab, transform);
                }

                var line = _sheetLineObjectLookup[i];
                line.transform.localPosition = new Vector3(beatSpacing * i, 0, 0);
            }
        }
    }
}