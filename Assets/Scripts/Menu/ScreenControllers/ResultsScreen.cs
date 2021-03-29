using System;
using System.Linq;
using Gameplay;
using Shared;
using Shared.Domain;
using TMPro;
using Tools;
using UnityEngine;

namespace Menu.ScreenControllers
{
    public class ResultsScreen : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textBlock;
        [SerializeField] private TextMeshProUGUI gradeText;

        public static BeatmapScore beatmapScore; // Injected by GameManager

        private BeatmapScore.ResultGrade _grade;

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
            _grade = beatmapScore.Grade;
            gradeText.text = _grade.ToString();
            gradeText.characterSpacing = _grade == BeatmapScore.ResultGrade.AAA ? -65 : 0;
        }

        private void Update()
        {
            gradeText.color = gradeText.color.SetAlpha(Anim.Pulsate((int) _grade, .4f));
        }

        public void BackToSongSelect()
        {
            MenuManager.Instance.ToScreen(MenuScreen.SongSelect);
        }
        
        private void OnEnable() => InputManager.PressConfirm += BackToSongSelect;
        private void OnDisable() => InputManager.PressConfirm -= BackToSongSelect;
    }
}
