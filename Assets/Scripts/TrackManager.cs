using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// todo: effect when player hits / misses / nears char, and make that persist in the char as it moves beyond the hantei
public class TrackManager : Singleton<TrackManager>
{
    [SerializeField] private RectTransform containerRectTransform = null;
    [NonSerialized] public Rect containerRect;
    
    private TrackNotesRecycler _notesRecycler;
    // todo: add sheet
    // private EditorTrackSheetBg _sheet;

    private const float _judgementOffsetX = 20;
    
    private bool _shouldDraw;
    
    private RuntimeWordObject _currentWordObj;

    public void InitTrack(Beatmap beatmap, List<RuntimeWord> words)
    {
        containerRect = containerRectTransform.rect;
        
        Debug.Log("init track");

        OnPan?.Invoke(_panX);
        
        _notesRecycler = TrackNotesRecycler.Instance;
        _notesRecycler.Init(words);
        
        // _sheet = EditorTrackSheetBg.Instance;
        // _sheet.InitSheet(beatmap);
        
        Pan(0f);
    }

    private void Update()
    {
        if (_shouldDraw)
        {
            _shouldDraw = false;
            // todo: only do this every so often
            _notesRecycler.RefreshWindow();
            // _sheet.DrawSheet();
        }
    }
    
    private float _beatSpacing = 20f; // todo: replace by some speed-based thing
    
    private void ChangeCurrentWord(float beat)
    {
        // todo: de-mark previous current word if there was still some current
        _currentWordObj = _notesRecycler.VisibleWordObjects[beat.BeatToIndex()];
        // If they're transparent by default:
        // foreach (var obj in _currentWordObjects)
        //     obj.color = Color.white;
    }
    
    private void ChangeCurrentChar(int? charIndex)
    {
        if (charIndex == null)
        {
            _currentWordObj.CharObjRefs.Last().color = Color.white;
            return;
        }

        var index = (int) charIndex;
            
        if (index >= _currentWordObj.CharObjRefs.Count)
            return;
        
        _currentWordObj.CharObjRefs[index].color = Color.red;
        if (index > 0)
            _currentWordObj.CharObjRefs[index-1].color = Color.white;
    }
    
    // todo: replace by more timing-like system
    private float _panX;

    public void Pan(float newPanX)
    {
        _panX = newPanX;
        transform.localPosition = new Vector3(_judgementOffsetX -_panX, 0, 0);
        OnPan?.Invoke(_panX);
        _shouldDraw = true;
    }

    private void OnEnable()
    {
        BeatmapManager.OnChangeCurrentWord += ChangeCurrentWord;
        BeatmapManager.OnChangeCurrentChar += ChangeCurrentChar;
    }
    
    public static event Action<float> OnPan;
}
