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

        private BeatmapResult? _beatmapResult;
    
        private void Start()
        {
            Invoke(nameof(ShowResult), 3f);
        }

        private void Update()
        {
            if (BeatmapManager.Instance.currentBeatmap == null)
                return;
            
            outputTextBox.text = string.Join(Environment.NewLine,
                "Diagnostics:",
                $"{BeatmapManager.Instance.currentBeatmap.bpm} BPM",
                $"{SongManager.Instance.SongPosSec} seconds in",
                $"{SongManager.Instance.SongPosBeatsMod} beats in",
                $"{SongManager.Instance.SongPosBars} bars in"
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
            _beatmapResult = BeatmapManager.Instance.GetResult();
        }

        private void OnEnable()
        {
            BeatmapManager.OnBeatmapSongFinished += ShowResult;
        }
    }
}
