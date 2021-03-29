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
        private Vector3 _shownPos;
        private Vector3 _hiddenPos;

        protected void Init(int direction, bool directionIsX = true)
        {
            _shownPos = transform.localPosition;
            rect = GetComponent<RectTransform>().rect;
            _hiddenPos = _shownPos + (directionIsX
                ? new Vector3(rect.width * direction, 0, 0)
                : new Vector3(0,rect.height * direction, 0));
            transform.localPosition = _hiddenPos;
        }

        public void ToggleOpened() => ToggleOpened(true);
        public virtual void ToggleOpened(bool triggerInputManager)
        {
            opened = !opened;
            transform.DOLocalMove(opened ? _shownPos : _hiddenPos, 1);

            if (opened)
                EditorInputManager.Instance.enabled = false;
            else if (triggerInputManager)
                EditorInputManager.Instance.enabled = true;

            if (triggerInputManager)
                DataController.TriggerToggleMenu(opened);
        }

        private void Close()
        {
            if (opened)
                ToggleOpened(true);
        }

        protected static readonly Regex FloatRegex = new Regex(@"^\d+(?:\.\d*)?$|^\.\d+$");
        protected static readonly Regex SignedFloatRegex = new Regex(@"^-?\d+(?:\.\d*)?$|^-?\.\d+$");
        protected static readonly Regex IntRegex = new Regex(@"^\d+$");
        protected static TMP_InputField.OnValidateInput ValidateInput(int maxLength = 200, Regex mustMatch = null)
        {
            return (text, index, addedChar) =>
            {
                var newText = text.Substring(0,index) + addedChar + text.Substring(index);
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

        protected void DisableShortcutsDuringInput(params TMP_InputField[] fields)
        {
            foreach (var field in fields)
            {
                field.onSelect.AddListener(_ => InputManager.Instance.enabled = false);
                field.onEndEdit.AddListener(_ => InputManager.Instance.enabled = true);
            }
        }

        private void OnEnable() => InputManager.PressBack += Close;
        private void OnDisable() => InputManager.PressBack -= Close;
    }

    public static class DataController
    {
        private static bool _opened;
        public static void TriggerToggleMenu(bool opened)
        {
            if (_opened == opened) return;
            _opened = opened;
            OnToggleMenu?.Invoke(_opened);
        }
        public static event Action<bool> OnToggleMenu;
    }
}