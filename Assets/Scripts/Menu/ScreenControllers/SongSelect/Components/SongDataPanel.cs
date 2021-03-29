using System;
using System.Linq;
using Menu.Components;
using Shared;
using Shared.Domain;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;
using Cache = Shared.Cache;

namespace Menu.ScreenControllers.SongSelect.Components
{
    public class SongDataPanel : MonoBehaviour
    {
        private static readonly Color TextColorSelected = C.CharColorHighlight;
        private static readonly Color TextColorUnselected = new Color(0f, 0.3f, 0f);

        [SerializeField] private TitleText songTitleText;
        [SerializeField] private TextMeshProUGUI songArtistText;
        [SerializeField] private RawImage jacketImage;
        [SerializeField] private LeetButton[] diffButtons;
        [SerializeField] private TextMeshProUGUI[] highscoreTexts;
        [SerializeField] private TextMeshProUGUI diffCreatorText;

        private Song _song;

        public event Action<SongDifficulty> OnChooseDiff;

        private int _selectedDiffIndex;

        private void SetHighscores(int[] scores)
        {
            for (var i = 0; i < highscoreTexts.Length; i++)
                highscoreTexts[i].text = i >= scores.Length ? "" : scores[i].ToString("D8");
        }

        private async void SetDiffDataAsync(SongDifficulty diff)
        {
            SetHighscores(new int[0]);
            var topScores = Local.Scores[diff.id].Reverse().Take(highscoreTexts.Length).Select(s => s.Score).ToArray();
            diffCreatorText.text = diff.creator;
            SetHighscores(topScores);
            jacketImage.texture = await diff.Jacket;
        }

        private void ToggleBtn(LeetButton btn, bool selected)
        {
            btn.SetColor(selected ? TextColorSelected : TextColorUnselected);
            diffButtons[_selectedDiffIndex].ambientGlowSpeed = selected ? 7f : 3f;
        }

        private void ChooseDiff(int index)
        {
            if (_selectedDiffIndex != index)
                ToggleBtn(diffButtons[_selectedDiffIndex], false);
            _selectedDiffIndex = index;
            ToggleBtn(diffButtons[index], true);

            SetDiffDataAsync(_song.diffs[index]);
            OnChooseDiff?.Invoke(_song.diffs[index]);
        }

        public void CycleDiff() =>
            diffButtons[(_selectedDiffIndex + 1) % _song.diffs.Length].OnClick.Invoke();

        private void OnHor(int direction) =>
            diffButtons[Mathf.Clamp(_selectedDiffIndex + direction, 0, _song.diffs.Length-1)].OnClick.Invoke();

        public void SetSong(Song song)
        {
            _song = song;
            songTitleText.Text = song.title;
            songArtistText.text = song.artist;
            for (var i = 0; i < diffButtons.Length; i++)
            {
                var btn = diffButtons[i];
                if (i >= song.diffs.Length)
                {
                    btn.SetInteractable(false);
                    btn.SetText("");
                    continue;
                }

                btn.SetInteractable(true);
                btn.SetText(song.diffs[i].DifficultyName.ToUpper());
            }
            
            ChooseDiff(0);
        }

        private void OnEnable()
        {
            foreach (var (btn, i) in diffButtons.WithIndex())
            {
                btn.OnClick.AddListener(() => ChooseDiff(i));
                ToggleBtn(btn, false);
            }
            MenuInputManager.PressHor += OnHor;
        }
        private void OnDisable()
        {
            MenuInputManager.PressHor -= OnHor;
        }
    }
}