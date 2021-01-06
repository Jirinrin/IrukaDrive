using System;
using System.Linq;
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
        private static readonly Color TextColorSelected = Color.green;
        private static readonly Color TextColorUnselected = new Color(0f, 0.5f, 0f);

        [SerializeField] private TextMeshProUGUI songTitleText;
        [SerializeField] private TextMeshProUGUI songArtistText;
        [SerializeField] private RawImage jacketImage;
        [SerializeField] private Button[] diffButtons;
        [SerializeField] private TextMeshProUGUI[] highscoreTexts;

        private TextMeshProUGUI[] _diffButtonTexts;

        private Song _song;

        public event Action<string> OnChooseDiff;

        private int _selectedDiffIndex;
        private void ChooseDiff(int index)
        {
            if (_selectedDiffIndex != index)
            {
                _diffButtonTexts[_selectedDiffIndex].color = TextColorUnselected;
                _selectedDiffIndex = index;
            }
            _diffButtonTexts[index].color = TextColorSelected;
            
            var beatmap = Cache.GetBeatmap(_song.diffPaths[index]);
            var top3Scores = Local.Scores[beatmap.id].Reverse().Take(3).Select(s => s.Score).ToArray();
            for (var i = 0; i < highscoreTexts.Length; i++)
                highscoreTexts[i].text = i >= top3Scores.Length ? "" : top3Scores[i].ToString("D8");

            OnChooseDiff?.Invoke(_song.diffPaths[index]);
        }

        private void OnEnable()
        {
            _diffButtonTexts = new TextMeshProUGUI[diffButtons.Length];
            foreach (var (btn, i) in diffButtons.WithIndex())
            {
                btn.onClick.AddListener(() => ChooseDiff(i));
                _diffButtonTexts[i] = btn.GetComponentInChildren<TextMeshProUGUI>();
                _diffButtonTexts[i].color = TextColorUnselected;
            }
        }

        public void SetSong(Song song)
        {
            _song = song;
            songTitleText.text = song.title;
            songArtistText.text = song.artist;
            jacketImage.texture = song.jacket;
            for (var i = 0; i < diffButtons.Length; i++)
            {
                var btn = diffButtons[i];
                var txt = _diffButtonTexts[i];
                if (i >= song.diffPaths.Length)
                {
                    btn.interactable = false;
                    txt.text = "";
                    continue;
                }

                btn.interactable = true;
                // todo: get the actual difficulty name stored in the beatmap (?)
                var btnText = song.diffPaths[i].Match(@"([^\\/]+)\.drive$")?.Groups[1].Value ?? "???";
                txt.text = btnText.ToUpper();
            }
            
            ChooseDiff(0);
        }
    }
}