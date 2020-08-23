using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [SerializeField] private GameObject judgementTransform;
    [SerializeField] private GameObject circlePrefab;

    private Dictionary<float, GameObject> _noteObjectLookup;

    public void InitTrack()
    {
        _noteObjectLookup = new Dictionary<float, GameObject>();
    }

    public void DrawTrackNotes(float timeBeats, IEnumerable<RuntimeNote> notes)
    {
        foreach (var note in notes)
        {
            // todo: recycle stuff
            if (!_noteObjectLookup.ContainsKey(note.Beat))
                _noteObjectLookup[note.Beat] = Instantiate(circlePrefab, judgementTransform.transform);

            var noteObject = _noteObjectLookup[note.Beat];
            var pos = note.Beat - timeBeats;
            
            noteObject.transform.localPosition = new Vector3(pos, 0, 0);
        }
    }
}
