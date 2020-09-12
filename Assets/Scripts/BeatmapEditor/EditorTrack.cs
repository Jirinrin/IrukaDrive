using System;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using TMPro;
using UnityEngine;

namespace BeatmapEditor
{
    // todo: maybe extract Sheet and have it subscribe to pan/zoom events
    public class EditorTrack : Singleton<EditorTrack>
    {
        [SerializeField] private GameObject characterPrefab;
        [SerializeField] private Line barLinePrefab;
        [SerializeField] private Line beatLinePrefab;
        
        [SerializeField] private RectTransform containerRectTransform;
        private Rect _containerRect;

        private GameObject _sheetParent;
        private GameObject _notesParent;
        private List<EditorWord> _words;
        private Dictionary<float, Dictionary<float, TextMeshProUGUI>> _noteObjectLookup;
        private Dictionary<int, Line> _sheetLineObjectLookup;

        private Beatmap _beatmap;
        
        public void InitTrack(Beatmap beatmap)
        {
            _beatmap = beatmap;
            _words = beatmap.words.Select(word => new EditorWord(word)).ToList();
            Debug.Log("init track");

            _containerRect = containerRectTransform.rect;
            
            _notesParent = GameObject.Find("EditorTrackNotes");
            
            _noteObjectLookup = new Dictionary<float, Dictionary<float, TextMeshProUGUI>>();
            foreach (var word in _words)
                _noteObjectLookup[word.Beat] = new Dictionary<float, TextMeshProUGUI>();
            
            Pan(0f);
            DrawTrackNotes();
            
            InitSheet();
        }

        private void InitSheet()
        {
            _sheetParent = GameObject.Find("EditorTrackSheet");
            _sheetLineObjectLookup = new Dictionary<int, Line>();
            
            var lineStartEndY = _containerRect.height / 2f - 10f;
            barLinePrefab.Start = new Vector3(0, lineStartEndY, 0);
            barLinePrefab.End = new Vector3(0, -lineStartEndY, 0);
            beatLinePrefab.Start = new Vector3(0, lineStartEndY, 0);
            beatLinePrefab.End = new Vector3(0, -lineStartEndY, 0);
            
            DrawSheetLines();
        }

        private void DrawTrackNotes()
        {
            foreach (var word in _words)
            {
                // todo: group characters by word (with a parent) or something
            
                var wordCharObjectsLookup = _noteObjectLookup[word.Beat];
            
                foreach (var charNote in word.CharNotes)
                {
                    // todo: recycle stuff
                    if (!wordCharObjectsLookup.ContainsKey(charNote.Beat))
                        wordCharObjectsLookup[charNote.Beat] = Instantiate(characterPrefab, _notesParent.transform).GetComponent<TextMeshProUGUI>();

                    var noteObject = wordCharObjectsLookup[charNote.Beat];
                    var pos = charNote.Beat * _beatSpacing;

                    // Debug.Log("crash place");
                    // Debug.Log(charNote);
                    // Debug.Log(noteObject);

                    noteObject.text = charNote.Char.ToString();
                    noteObject.transform.localPosition = new Vector3(pos, 0, 0);
                }
            }
        }

        private void DrawSheetLines()
        {
            var minBeat = Mathf.Max(_panX / _beatSpacing, 0f);
            var maxBeat = minBeat + _containerRect.width / _beatSpacing;
            
            for (var i = Mathf.FloorToInt(minBeat); i < Mathf.CeilToInt(maxBeat); i++)
            {
                // todo: recycle stuff
                if (!_sheetLineObjectLookup.ContainsKey(i))
                {
                    var linePrefab = i % _beatmap.beatsPerBar == _beatmap.barOffset ? barLinePrefab : beatLinePrefab;
                    _sheetLineObjectLookup[i] = Instantiate(linePrefab, _sheetParent.transform);
                }

                var line = _sheetLineObjectLookup[i];
                line.transform.localPosition = new Vector3(_beatSpacing * i, 0, 0);
            }
        }

        private const float DefaultBeatSpacing = 20f;
        private const float MaxZoomScale = 5f;
        private float _scaleX = 1f;
        private float _beatSpacing = DefaultBeatSpacing;
        public void Zoom(float delta, float screenPivotX)
        {
            var oldScale = _scaleX;
            _scaleX = Mathf.Clamp(_scaleX + delta, 1f, MaxZoomScale);
            _beatSpacing = _scaleX * DefaultBeatSpacing;
            var scaleDiff = _scaleX / oldScale;
            
            var pivotX = _panX + screenPivotX;
            Pan(-pivotX * (scaleDiff-1f));
            DrawTrackNotes();
            DrawSheetLines();
        }

        // panX gets inversed to make 'the pan amount' more intuitive to deal with
        private const float MinimumPan = -20f;
        private float _panX = MinimumPan;
        public void Pan(float deltaX)
        {
            _panX = Mathf.Max(_panX - deltaX, MinimumPan);
            transform.localPosition = new Vector3(-_panX, 0, 0);
            DrawSheetLines();
        }
    }
}
