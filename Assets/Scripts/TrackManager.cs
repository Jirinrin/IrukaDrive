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
    private List<TextMeshPro> _currentWordObjects;

    public void InitTrack(List<RuntimeWord> words)
    {
        Debug.Log("init track");
        _words = words;
        _noteObjectLookup = new Dictionary<float, Dictionary<float, TextMeshPro>>();
        foreach (var word in words)
        {
            _noteObjectLookup[word.Beat] = new Dictionary<float, TextMeshPro>();
        }
    }

    public void DrawTrackNotes(float timeBeats)
    {
        foreach (var word in _words)
        {
            // todo: group characters by word (with a parent) or something
            
            var wordObjectsLookup = _noteObjectLookup[word.Beat];
            
            foreach (var charNote in word.CharNotes)
            {
                // todo: recycle stuff
                if (!wordObjectsLookup.ContainsKey(charNote.Beat))
                    wordObjectsLookup[charNote.Beat] = Instantiate(characterPrefab, judgementTransform.transform).GetComponent<TextMeshPro>();

                var noteObject = wordObjectsLookup[charNote.Beat];
                var pos = charNote.Beat - timeBeats;
                
                noteObject.text = charNote.Char.ToString();
                noteObject.transform.localPosition = new Vector3(pos, 0, 0);
            }
        }
    }
    
    private void ChangeCurrentWord(float beat)
    {
        _currentWordObjects = _noteObjectLookup[beat].Values.ToList();
        // If they're transparent by default:
        // foreach (var obj in _currentWordObjects)
        // {
        //     obj.color = Color.white;
        // }
    }
    
    private void ChangeCurrentChar(int index)
    {
        if (index >= _currentWordObjects.Count)
            return;
        
        _currentWordObjects[index].color = Color.red;
        if (index > 0)
            _currentWordObjects[index-1].color = Color.white;
    }

    private void OnEnable()
    {
        BeatmapManager.OnChangeCurrentWord += ChangeCurrentWord;
        BeatmapManager.OnChangeCurrentChar += ChangeCurrentChar;
    }
}
