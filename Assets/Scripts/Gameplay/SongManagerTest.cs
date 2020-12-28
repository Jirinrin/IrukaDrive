using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Domain;
using Shared;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class SongManagerTest : MonoBehaviour
    {
        public Text outputTextBox;
        public Text resultsTextBox;

        private IEnumerable<RuntimeNote> _beatmapResult;
    
        private void Start()
        {
            Invoke(nameof(ShowResult), 3f);
        }

        private void Update()
        {
            if (!GameplayManager.Instance.beatmapStarted)
                return;
            
            outputTextBox.text = string.Join(Environment.NewLine,
                "Diagnostics:",
                $"{GameplayManager.CurrentBeatmap.bpm} BPM",
                $"{SongManager.Instance.songPosSec} seconds in",
                $"{SongManager.Instance.SongPosBeatsMod} beats in",
                $"{SongManager.Instance.SongPosBars} bars in"
            );
            ShowResult();
            if (_beatmapResult != null)
                resultsTextBox.text = string.Join(Environment.NewLine,
                    "Results:",
                    string.Join(", ", _beatmapResult.Where(n => n.result != null).Select(note => 
                        $"[{note.result} - {note.resultTiming}]"
                    ))
                );
        }

        private void ShowResult() => _beatmapResult = GameplayManager.RuntimeWords.GetNotes();
    }
}
