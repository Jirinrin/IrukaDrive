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
                $"Score: {beatmapScore.Score:D8}",
                $"Perfect: {beatmapScore.perfects}",
                $"Early: {beatmapScore.earlies}",
                $"Late: {beatmapScore.lates}",
                $"Miss: {beatmapScore.misses}",
                $"Max combo: {beatmapScore.maxCombo}"
                // ,string.Join(", ", GameplayManager.RuntimeWords.GetNotes().Select(note =>
                //     $"[{note.Result} - {note.ResultTiming}]"
                // ))
            );
        }

        public void BackToSongSelect()
        {
            MenuManager.Instance.ToScreen("SongSelect");
        }
    }
}
