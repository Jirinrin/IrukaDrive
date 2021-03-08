using System;
using BeatmapEditor.Domain;
using BeatmapEditor.SingletonComponents;
using Tools;
using Tools.Commons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatmapEditor
{
    [RequireComponent(typeof(PlayerInput))]
    public class EditorInputManager : Singleton<EditorInputManager>
    {
        // 略 for IfEnabled
        private void IE(Action todo)
        {
            if (enabled) todo();
        }

        public void OnSave() => IE(BeatmapEditorManager.Instance.SaveBeatmap);

        private void OnPlay() =>
            IE(() => EditorTrack.Instance.PlayFromPoint(Input.mousePosition.x * ScreenToCanvas.Factor, !Keyboard.current.shiftKey.isPressed));
        
        public void OnCopy() => IE(EditorTrackClipboard.Instance.Copy);
        public void OnPaste() => IE(EditorTrackClipboard.Instance.Paste);
        public void OnDelete() => IE(EditorTrackClipboard.Instance.Delete);
        public void OnRename() => IE(EditorTrackClipboard.Instance.Rename);
        public void OnNewWord() => IE(EditorTrackClipboard.Instance.CreateWord);

        public void OnUndo() => IE(EditorHistory.Undo);
        public void OnRedo() => IE(EditorHistory.Redo);

        private void OnEnable() => EditorTrackGestures.Instance.enabled = true;
        private void OnDisable() => EditorTrackGestures.Instance.enabled = false;
    }
}