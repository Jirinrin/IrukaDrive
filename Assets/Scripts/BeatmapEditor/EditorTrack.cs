using System;
using System.Collections.Generic;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;

namespace BeatmapEditor
{
    public class EditorTrack : Singleton<EditorTrack>
    {
        [SerializeField] private GameObject characterPrefab;

        private List<EditorWord> _words;
        private Dictionary<float, Dictionary<float, TextMeshProUGUI>> _noteObjectLookup;
        
        public void InitTrack(List<EditorWord> words)
        {
            _words = words;
            Debug.Log("init track");
            
            _noteObjectLookup = new Dictionary<float, Dictionary<float, TextMeshProUGUI>>();
            foreach (var word in _words)
            {
                _noteObjectLookup[word.Beat] = new Dictionary<float, TextMeshProUGUI>();
            }
            DrawTrackNotes();
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
                        wordCharObjectsLookup[charNote.Beat] = Instantiate(characterPrefab, transform).GetComponent<TextMeshProUGUI>();

                    var noteObject = wordCharObjectsLookup[charNote.Beat];
                    var pos = charNote.Beat * DefaultSpacing * _scaleX;

                    // Debug.Log("crash place");
                    // Debug.Log(charNote);
                    // Debug.Log(noteObject);

                    noteObject.text = charNote.Char.ToString();
                    noteObject.transform.localPosition = new Vector3(pos, 0, 0);
                }
            }
        }

        private void DrawLines()
        {
            var path = new PathProperties();
            // Line.Instantiate()
        }

        private const float DefaultSpacing = 20f;
        private const float MaxZoomScale = 5f;
        private float _scaleX = 1f;
        public void Zoom(float delta, float screenPivotX)
        {
            var oldScale = _scaleX;
            _scaleX = Mathf.Clamp(_scaleX + delta, 1f, MaxZoomScale);
            var scaleDiff = _scaleX / oldScale;
            

            var pivotX = _panX + screenPivotX;
            Pan(-pivotX * (scaleDiff-1f));
            DrawTrackNotes();
        }

        // panX gets inversed to make 'the pan amount' more intuitive to deal with
        private const float MinimumPan = 20f;
        private float _panX = MinimumPan;
        public void Pan(float deltaX)
        {
            _panX = Mathf.Max(_panX - deltaX, MinimumPan); // todo: clamp at track fitting in screen
            transform.localPosition = new Vector3(-_panX, 0, 0);
        }
    }
}
