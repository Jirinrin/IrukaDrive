using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Domain;
using Shared;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.SingletonComponents
{
    public class DiagnosticsHud : MonoBehaviour
    {
        public Text outputTextBox;
        public Text resultsTextBox;

        private IEnumerable<RuntimeNote> _beatmapResult;
    
        private void Start()
        {
            InvokeRepeating(nameof(UpdateDisplay), 0f, 1f);
        }

        private void UpdateDisplay()
        {
            if (!GameplayManager.Instance.beatmapStarted)
                return;
            
            _beatmapResult = GameplayManager.RuntimeWords.GetNotes();
            outputTextBox.text = string.Join(Environment.NewLine,
                "Diagnostics:",
                $"{GameplayManager.CurrentBeatmap.bpm} BPM",
                $"{SongManager.Instance.songPosSec} secs",
                $"{SongManager.Instance.SongPosBeatsMod} beats",
                $"{SongManager.Instance.SongPosBars} bars"
            );
            if (_beatmapResult != null)
                resultsTextBox.text = string.Join(Environment.NewLine,
                    "Results:",
                    string.Join(", ", _beatmapResult.Where(n => n.result != null).Select(note => 
                        $"[{note.result} - {note.resultTiming}]"
                    )),
                    "Max Combo:",
                    GameplayManager.Instance.displayScore.maxCombo
                );
        }
    }
}
