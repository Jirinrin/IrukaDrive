using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Domain;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace BeatmapEditor.SingletonComponents
{
    public class ChartDataController : DataController<ChartDataController>
    {
        [SerializeField] private TMP_Dropdown difficultyDropdown;
        [SerializeField] private TMP_InputField difficultyNameOverrideField;
        [SerializeField] private TMP_InputField creatorField;
        [SerializeField] private TMP_InputField finishTimestampField;
        [SerializeField] private Button jacketFileOverrideButton;
        [SerializeField] private Button jacketFileOverrideResetButton;

        private static Beatmap B => BeatmapEditorManager.currentBeatmap;

        private void Start()
        {
            Init(1);

            difficultyDropdown.options = new List<TMP_Dropdown.OptionData>(Enum.GetValues(typeof(Difficulty)).Cast<Difficulty>().Select(d => new TMP_Dropdown.OptionData(d.ToString())));
            difficultyDropdown.onValueChanged.AddListener(v => B.difficulty = (Difficulty) v);

            creatorField.onValueChanged.AddListener(v => B.creator = v);
            creatorField.onValidateInput += ValidateInput(50);

            difficultyNameOverrideField.onValueChanged.AddListener(v => B.difficultyNameOverride = v.OrNull());
            difficultyNameOverrideField.onValidateInput += ValidateInput(10);

            finishTimestampField.onValueChanged.AddListener(v =>
            {
                B.finishTimestamp = v.IsNullOrEmpty() ? (float?) null : float.Parse(v);
                EditorEndMark.Instance.UpdatePos();
            });
            finishTimestampField.onValidateInput += ValidateInput(6, FloatRegex);

            jacketFileOverrideButton.onClick.AddListener(() => AddFile(p =>
            {
                B.jacketFileOverride = p;
                jacketFileOverrideResetButton.gameObject.SetActive(true);
            }, B.song.folderPath, ext => SerializationHelperUtils.ImageExtensionRegex.IsMatch(ext.Substring(1).ToLower())));
            jacketFileOverrideResetButton.onClick.AddListener(() =>
            {
                B.jacketFileOverride = null;
                jacketFileOverrideResetButton.gameObject.SetActive(false);
            });

            DisableShortcutsDuringInput(creatorField, difficultyNameOverrideField, finishTimestampField);
        }

        public override void ToggleOpened(bool t)
        {
            base.ToggleOpened(t);
            if (opened)
            {
                if (SongDataController.Instance.opened)
                    SongDataController.Instance.ToggleOpened(false);
                if (HelpScreenController.Instance.opened)
                    HelpScreenController.Instance.ToggleOpened(false);

                difficultyDropdown.value = (int) B.difficulty;
                creatorField.text = B.creator;
                difficultyNameOverrideField.text = B.difficultyNameOverride;
                finishTimestampField.text = B.finishTimestamp.ToString();
                jacketFileOverrideResetButton.gameObject.SetActive(!B.jacketFileOverride.IsNullOrEmpty());
            }
        }
    }
}