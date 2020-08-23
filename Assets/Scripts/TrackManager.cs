using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [SerializeField] private GameObject judgementTransform;
    [SerializeField] private GameObject characterPrefab;

    private Dictionary<float, TextMeshPro> _noteObjectLookup;

    public void InitTrack()
    {
        _noteObjectLookup = new Dictionary<float, TextMeshPro>();
    }

    public void DrawTrackNotes(float timeBeats, IEnumerable<RuntimeNote> notes)
    {
        foreach (var note in notes)
        {
            // todo: recycle stuff
            if (!_noteObjectLookup.ContainsKey(note.Beat))
                _noteObjectLookup[note.Beat] = Instantiate(characterPrefab, judgementTransform.transform).GetComponent<TextMeshPro>();

            var noteObject = _noteObjectLookup[note.Beat];
            var pos = note.Beat - timeBeats;
            
            noteObject.text = note.Char.ToString();
            noteObject.transform.localPosition = new Vector3(pos, 0, 0);
        }
    }
}
