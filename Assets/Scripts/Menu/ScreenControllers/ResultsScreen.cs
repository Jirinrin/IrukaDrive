using System;
using System.Linq;
using Gameplay;
using Shared.Domain;
using TMPro;
using Tools;
using UnityEngine;

namespace Menu.ScreenControllers
{
    public class ResultsScreen : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textBlock;

        public static BeatmapScore beatmapScore; // Injected by GameManager

        private void Start()
        {
            textBlock.text = string.Join(Environment.NewLine,
                "Results:",
                string.Join(", ", GameplayManager.RuntimeWords.GetResults().Select(note => 
                    $"[{note.Result} - {note.ResultTiming}]"
                ))
            );
        }

        public void BackToSongSelect()
        {
            MenuManager.Instance.ToScreen("Title");
        }
    }
}
