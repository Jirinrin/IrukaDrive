using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatmapManager : MonoBehaviour
{
    private SongManager _songManager;
    // [NonSerialized] public Beatmap currentBeatmap;
    
    private void Start()
    {
        _songManager = GetComponent<SongManager>();
    }

    private void Update()
    {
        
    }

    public void Tap()
    {
        
    }
}
