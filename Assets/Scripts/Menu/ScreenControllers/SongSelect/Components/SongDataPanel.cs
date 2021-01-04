using System;
using Shared.Domain;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

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

        private readonly TextMeshProUGUI[] _diffButtonTxts = new TextMeshProUGUI[4];

        private Song _song;

        public event Action<string> OnChooseDiff;

        private int _selectedDiffIndex;
        private void ChooseDiff(int index)
        {
            if (_selectedDiffIndex != index)
            {
                _diffButtonTxts[_selectedDiffIndex].color = TextColorUnselected;
                _selectedDiffIndex = index;
            }
            _diffButtonTxts[index].color = TextColorSelected;
            
            OnChooseDiff?.Invoke(_song.diffPaths[index]);
        }

        private void Start()
        {
            foreach (var (btn, i) in diffButtons.WithIndex())
            {
                btn.onClick.AddListener(() => ChooseDiff(i));
                _diffButtonTxts[i] = btn.GetComponentInChildren<TextMeshProUGUI>();
                _diffButtonTxts[i].color = TextColorUnselected;
            }
        }

        public void SetSong(Song song)
        {
            _song = song;
            songTitleText.text = song.title;
            songArtistText.text = song.artist;
            jacketImage.texture = song.jacket;
            for (var i = 0; i < 4; i++)
            {
                var btn = diffButtons[i];
                var txt = _diffButtonTxts[i];
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