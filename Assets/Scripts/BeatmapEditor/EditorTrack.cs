using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BeatmapEditor
{
    // todo: maybe extract Sheet and have it subscribe to pan/zoom events
    public class EditorTrack : Singleton<EditorTrack>
    {
        [SerializeField] private GameObject characterPrefab;
        
        [SerializeField] private RectTransform containerRectTransform;
        [NonSerialized] public Rect containerRect;

        private EditorTrackSheetBg _sheet;

        private GameObject _notesParent;
        private List<EditorWord> _words;
        private Dictionary<float, Dictionary<float, TextMeshProUGUI>> _noteObjectLookup;

        public Beatmap _beatmap;

        private bool _shouldDraw;
        
        public void InitTrack(Beatmap beatmap)
        {
            _beatmap = beatmap;
            _words = beatmap.words.Select(word => new EditorWord(word)).ToList();
            Debug.Log("init track");

            containerRect = containerRectTransform.rect;
            
            _notesParent = GameObject.Find("EditorTrackNotes");
            
            _noteObjectLookup = new Dictionary<float, Dictionary<float, TextMeshProUGUI>>();
            foreach (var word in _words)
                _noteObjectLookup[word.Beat] = new Dictionary<float, TextMeshProUGUI>();

            OnPan?.Invoke(_panX);
            OnZoom?.Invoke(_beatSpacing);
            
            _sheet = EditorTrackSheetBg.Instance;
            _sheet.InitSheet();
            // todo: handle doing this in a more elegant place
            SheetLineRecyclerList.Instance.Init(_beatmap);
            
            DrawTrackNotes();
            _sheet.DrawSheet();
            
            Pan(0f);
            Zoom(0f, 0f);
        }

        private void Update()
        {
            if (_shouldDraw)
            {
                _shouldDraw = false;
                DrawTrackNotes();
                _sheet.DrawSheet();
            }
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

                    noteObject.text = charNote.Char.ToString();
                    noteObject.transform.localPosition = new Vector3(pos, 0, 0);
                }
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
            _shouldDraw = true;
            OnZoom?.Invoke(_beatSpacing);
        }

        // panX gets inversed to make 'the pan amount' more intuitive to deal with
        private const float MinimumPan = -20f;
        private float _panX = MinimumPan;
        public void Pan(float deltaX)
        {
            _panX = Mathf.Max(_panX - deltaX, MinimumPan);
            transform.localPosition = new Vector3(-_panX, 0, 0);
            OnPan?.Invoke(_panX);
        }

        public static event Action<float> OnPan;
        public static event Action<float> OnZoom;
    }
}
