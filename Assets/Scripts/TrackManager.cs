using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// todo: effect when player hits / misses / nears char, and make that persist in the char as it moves beyond the hantei
public class TrackManager : MonoBehaviour
{
    [SerializeField] private GameObject judgementTransform;
    [SerializeField] private GameObject characterPrefab;

    private List<RuntimeWord> _words;
    // todo: check if float as dictionary key works / doesn't result in weird memory leaks
    private Dictionary<float, Dictionary<float, TextMeshPro>> _noteObjectLookup;
    private List<TextMeshPro> _currentWordCharObjects;

    public void InitTrack(List<RuntimeWord> words)
    {
        _words = words;
        _noteObjectLookup = new Dictionary<float, Dictionary<float, TextMeshPro>>();
        foreach (var word in words)
        {
            _noteObjectLookup[word.Beat] = new Dictionary<float, TextMeshPro>();
        }
        // To instantiate the necessary game objects -- this will be made more elegant in the future
        DrawTrackNotes(0f);
    }

    // todo: have basically just the whole track move instead of each object individually
    public void DrawTrackNotes(float timeBeats)
    {
        foreach (var word in _words)
        {
            // todo: group characters by word (with a parent) or something
            
            var wordCharObjectsLookup = _noteObjectLookup[word.Beat];
            
            foreach (var charNote in word.CharNotes)
            {
                // todo: recycle stuff
                if (!wordCharObjectsLookup.ContainsKey(charNote.Beat))
                    wordCharObjectsLookup[charNote.Beat] = Instantiate(characterPrefab, judgementTransform.transform).GetComponent<TextMeshPro>();

                var noteObject = wordCharObjectsLookup[charNote.Beat];
                var pos = charNote.Beat - timeBeats;
                
                noteObject.text = charNote.Char.ToString();
                noteObject.transform.localPosition = new Vector3(pos, 0, 0);
            }
        }
    }
    
    private void ChangeCurrentWord(float beat)
    {
        _currentWordCharObjects = _noteObjectLookup[beat].Values.ToList();
        // If they're transparent by default:
        // foreach (var obj in _currentWordObjects)
        // {
        //     obj.color = Color.white;
        // }
    }
    
    private void ChangeCurrentChar(int? charIndex)
    {
        if (charIndex == null)
        {
            _currentWordCharObjects.Last().color = Color.white;
            return;
        }

        var index = (int) charIndex;
            
        if (index >= _currentWordCharObjects.Count)
            return;
        
        _currentWordCharObjects[index].color = Color.red;
        if (index > 0)
            _currentWordCharObjects[index-1].color = Color.white;
    }

    private void OnEnable()
    {
        BeatmapManager.OnChangeCurrentWord += ChangeCurrentWord;
        BeatmapManager.OnChangeCurrentChar += ChangeCurrentChar;
    }
}
