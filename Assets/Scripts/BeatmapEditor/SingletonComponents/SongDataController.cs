using System.Globalization;
using System.Text.RegularExpressions;
using Shared.Domain;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace BeatmapEditor.SingletonComponents
{
    public class SongDataController : DataController<SongDataController>
    {
        [SerializeField] private TMP_InputField titleField;
        [SerializeField] private TMP_InputField artistField;
        [SerializeField] private Button audioFileButton;
        [SerializeField] private Button jacketFileButton;

        [SerializeField] private TMP_InputField bpmField;
        [SerializeField] private TMP_InputField beatOffsetField;
        [SerializeField] private TMP_InputField beatsPerBarField;
        [SerializeField] private TMP_InputField barOffsetField;

        private static Song S => BeatmapEditorManager.currentSong;

        private void Start()
        {
            Init(-1);

            titleField.onValueChanged.AddListener(v => S.title = v);
            titleField.onValidateInput += ValidateInput(100);
            artistField.onValueChanged.AddListener(v => S.artist = v);
            artistField.onValidateInput += ValidateInput(100);

            audioFileButton.onClick.AddListener(() => AddFile(p => S.audioFile = p, S.folderPath, ext => SerializationHelperUtils.GetAudioType(ext) != AudioType.UNKNOWN));
            jacketFileButton.onClick.AddListener(() => AddFile(p => S.jacketFile = p, S.folderPath, ext => SerializationHelperUtils.ImageExtensionRegex.IsMatch(ext.Substring(1).ToLower())));

            bpmField.onValueChanged.AddListener(IfNotEmpty(SetWithRefresh((string v) => S.bpm = float.Parse(v))));
            bpmField.onValidateInput += ValidateInput(10, FloatRegex);
            beatOffsetField.onValueChanged.AddListener(IfNotEmpty(SetWithRefresh((string v) => S.beatOffset = float.Parse(v))));
            beatOffsetField.onValidateInput += ValidateInput(10, FloatRegex);
            beatsPerBarField.onValueChanged.AddListener(IfNotEmpty(SetWithRefresh((string v) => S.beatsPerBar = int.Parse(v))));
            beatsPerBarField.onValidateInput += ValidateInput(1, new Regex(@"[2-4]"));
            barOffsetField.onValueChanged.AddListener(IfNotEmpty(SetWithRefresh((string v) => S.barOffset = int.Parse(v))));
            barOffsetField.onValidateInput += ValidateInput(1, new Regex(@"[0-3]"));
        }

        public override void ToggleOpened(bool t)
        {
            base.ToggleOpened(t);
            if (opened)
            {
                if (ChartDataController.Instance.opened)
                    ChartDataController.Instance.ToggleOpened(false);
                if (HelpScreenController.Instance.opened)
                    HelpScreenController.Instance.ToggleOpened(false);

                titleField.text = S.title;
                artistField.text = S.artist;
                bpmField.text = S.bpm.ToString(CultureInfo.InvariantCulture);
                beatOffsetField.text = S.beatOffset.ToString(CultureInfo.InvariantCulture);
                beatsPerBarField.text = S.beatsPerBar.ToString();
                barOffsetField.text = S.barOffset.ToString();
            }
        }
    }
}