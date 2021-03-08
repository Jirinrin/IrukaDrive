using System;
using BeatmapEditor.Components;
using BeatmapEditor.SingletonComponents;
using JetBrains.Annotations;
using Shapes;
using Tools.Commons;
using UnityEngine;

namespace BeatmapEditor.Domain
{
    // todo: on init of a word that was selected it should look selected
    public class EditorTrackClipboard : Singleton<EditorTrackClipboard>
    {
        [SerializeField] private Line cursorLinePrefab = null;
        
        [CanBeNull] private static EditorWord _wordOnClipboard;

        private float _cursorPos;
        [CanBeNull] private EditorWordObject _selectedWord;

        private bool _initted;
        private Line _cursorLine;

        public void Init()
        {
            if (_initted)
            {
                Debug.LogWarning("Editor Track Clipboard already initted");
                return;
            }
            var lineStartEndY = EditorTrack.Instance.containerRect.height / 2f - 2f;
            _cursorLine = Instantiate(cursorLinePrefab, transform);
            _cursorLine.Start = new Vector3(0, lineStartEndY, 0);
            _cursorLine.End = new Vector3(0, -lineStartEndY, 0);
            _initted = true;
            UpdateCursorPos();
        }

        private void Update()
        {
            if (_initted)
                _cursorLine.DashOffset = Time.time;
        }

        private void UpdateCursorPos()
        {
            if (!_initted) return;
            _cursorLine.transform.localPosition = new Vector3(EditorTrack.viewState.beatSpacing * _cursorPos, 0, 0);
        }
        
        public void SetCursor(float screenX)
        {
            _cursorPos = EditorTrack.ScreenXToBeat(screenX, EditorUtils.GetEditorBeatSnap());
            UpdateCursorPos();
        }
        
        private void OnDestroySelectedWord()
        {
            if (_selectedWord == null) return;
            _selectedWord.OnDestroy -= OnDestroySelectedWord;
            _selectedWord = null;
        }
        
        public void SelectWord([CanBeNull] EditorWordObject word)
        {
            if (_selectedWord != null) _selectedWord.Selected = false;
            _selectedWord = word;
            if (_selectedWord != null)
            {
                _selectedWord.OnDestroy += OnDestroySelectedWord;
                _selectedWord.Selected = true;
            }
        }
        
        public void Copy()
        {
            if (_selectedWord == null)
                return;
            
            _wordOnClipboard = _selectedWord.word;
            // todo: display word copied msg / display the clipboard somewhere
        }
        public void Paste()
        {
            if (_wordOnClipboard == null || !EditorTrack.Instance.BeatIsVisible(_cursorPos))
                return;

            EditorTrack.Instance.AddWord(_wordOnClipboard.CloneWord(_cursorPos));
        }

        public void Delete()
        {
            if (_selectedWord == null) return;
            _selectedWord.Delete();
            _selectedWord = null;
        }

        public void Rename()
        {
            if (_selectedWord == null) return;
            _selectedWord.Edit();
        }

        public void CreateWord() => EditorTrack.Instance.CreateWord(_cursorPos, true);

        private void OnEnable()
        {
            EditorTrackViewState.OnZoom += UpdateCursorPos;
        }
        private void OnDisable()
        {
            EditorTrackViewState.OnZoom -= UpdateCursorPos;
        }
    }
}