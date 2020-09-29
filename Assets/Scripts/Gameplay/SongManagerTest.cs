using System;
using System.Linq;
using Shared.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class SongManagerTest : MonoBehaviour
    {
        public Text outputTextBox;
        public Text resultsTextBox;
        
        private SongManager _songManager;
        private BeatmapManager _beatmapManager;

        private BeatmapResult? _beatmapResult;
    
        private void Start()
        {
            _songManager = GetComponent<SongManager>();
            _beatmapManager = GetComponent<BeatmapManager>();
            Invoke(nameof(ShowResult), 3f);
        }

        private void Update()
        {
            outputTextBox.text = string.Join(Environment.NewLine,
                "Diagnostics:",
                $"{_beatmapManager.currentBeatmap.bpm} BPM",
                $"{_songManager.SongPosSec} seconds in",
                $"{_songManager.SongPosBeatsMod} beats in",
                $"{_songManager.SongPosBars} bars in"
            );
            ShowResult();
            if (_beatmapResult != null)
                resultsTextBox.text = string.Join(Environment.NewLine,
                    "Results:",
                    string.Join(", ", _beatmapResult?.NoteResults.Select(note => 
                        $"[{note.Result} - {note.ResultTiming}]"
                    ))
                );
        }

        private void ShowResult()
        {
            _beatmapResult = _beatmapManager.GetResult();
        }

        private void OnEnable()
        {
            BeatmapManager.OnBeatmapSongFinished += ShowResult;
        }
    }
}
