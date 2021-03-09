using System;
using System.IO;
using System.Text.RegularExpressions;
using DG.Tweening;
using Shared;
using SimpleFileBrowser;
using TMPro;
using Tools;
using Tools.Commons;
using UnityEngine;
using UnityEngine.Events;

namespace BeatmapEditor.SingletonComponents
{
    public class DataController<T> : Singleton<T> where T : MonoBehaviour
    {
        [NonSerialized] public bool opened;
        [NonSerialized] protected Rect rect;
        private int _direction;

        protected void Start(int direction)
        {
            _direction = direction;
            rect = GetComponent<RectTransform>().rect;
            transform.localPosition = new Vector3(rect.width*_direction, 0, 0);
        }

        public void ToggleOpened() => ToggleOpened(true);
        public virtual void ToggleOpened(bool triggerInputManager)
        {
            opened = !opened;
            transform.DOLocalMoveX(opened ? 0 : rect.width*_direction, 1);
            if (opened)
                EditorInputManager.Instance.enabled = false;
            else if (triggerInputManager)
                EditorInputManager.Instance.enabled = true;
        }

        private void Close()
        {
            if (opened)
                ToggleOpened(true);
        }

        protected static readonly Regex FloatRegex = new Regex(@"^(?:\d+)(?:\.\d*)?$");
        protected static readonly Regex IntRegex = new Regex(@"^\d+$");
        protected static TMP_InputField.OnValidateInput ValidateInput(int maxLength = 200, Regex mustMatch = null)
        {
            return (text, index, addedChar) =>
            {
                var newText = text + addedChar;
                if (text.Length == 0 && char.IsWhiteSpace(addedChar)
                    || text.Length >= maxLength
                    || (!mustMatch?.IsMatch(newText) ?? false)
                )
                    return '\0';

                return addedChar;
            };
        }

        protected void AddFile(Action<string> setter, string folderPath, Func<string, bool> extValidator = null)
        {
            FileBrowser.ShowLoadDialog(p =>
                {
                    var path = p[0];
                    var ext = Path.GetExtension(path);
                    if (!(extValidator?.Invoke(ext) ?? false))
                        return;

                    var newPath = Path.Combine(folderPath, Path.GetFileName(path));
                    if (path != newPath)
                    {
                        if (File.Exists(newPath))
                            newPath = newPath.Replace(ext, " (1)" + ext);
                        if (File.Exists(newPath))
                            return;
                        File.Copy(path, newPath);
                    }

                    setter(Path.GetFileName(newPath));
                },
                null,
                FileBrowser.PickMode.Files,false, folderPath, title: "Select a file");
        }

        protected UnityAction<string> IfNotEmpty(UnityAction<string> setter) =>
            v =>
            {
                if (!v.IsNullOrEmpty()) setter(v);
            };

        protected UnityAction<TType> SetWithRefresh<TType>(Action<TType> setter) =>
            v =>
            {
                setter(v);
                BeatmapEditorManager.Instance.RefreshBeatmap();
            };

        private void OnEnable() => InputManager.PressBack += Close;
        private void OnDisable() => InputManager.PressBack -= Close;
    }
}