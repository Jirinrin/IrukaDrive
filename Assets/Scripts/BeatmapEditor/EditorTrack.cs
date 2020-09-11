using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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
            Debug.Log("init");
            
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
                    Debug.Log(charNote.Beat);
                    // todo: recycle stuff
                    if (!wordCharObjectsLookup.ContainsKey(charNote.Beat))
                        wordCharObjectsLookup[charNote.Beat] = Instantiate(characterPrefab, transform).GetComponent<TextMeshProUGUI>();

                    var noteObject = wordCharObjectsLookup[charNote.Beat];
                    var pos = charNote.Beat * _spacing;

                    // Debug.Log("crash place");
                    // Debug.Log(charNote);
                    // Debug.Log(noteObject);

                    noteObject.text = charNote.Char.ToString();
                    noteObject.transform.localPosition = new Vector3(pos, 0, 0);
                }
            }
        }

        private float _spacing = 20f;
        public void Zoom(float delta)
        {
            _spacing += delta;
            DrawTrackNotes();
        }

        public void Pan(float deltaX)
        {
            var currentTransform = transform;
            currentTransform.localPosition = new Vector3(currentTransform.localPosition.x + deltaX, 0, 0);
        }
    }
}
