using System;
using BeatmapEditor.Domain;
using BeatmapEditor.SingletonComponents;
using Shared;
using Shared.Domain;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace BeatmapEditor.Components
{
    public class EditorWordObject : WordObject<EditorWord, ParsedChar>, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        [NonSerialized] public TMP_InputField inputFieldPrefab;
        
        private float _dragDelta;
        private bool _dragging;
        private float _xBeforeDrag;
        private DragType _dragType;
        private int _beatIntervalIndexBeforeDrag;

        private TMP_InputField _activeInputField;
        private static bool _inputting;

        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                SetColor(_selected ? C.CharColorHighlight : C.CharColorDefaultEditor);
            }
        }

        // Constants and stuff
        
        private const float DragXToIndexShift = .1f;

        private enum DragType
        {
            MoveWord,
            ChangeBeatInterval,
        }

        private void UpdateBeatInterval(float newInterval)
        {
            word.BeatInterval = newInterval;
            RefreshWord();
        }

        public void Edit()
        {
            if (_inputting)
            {
                Debug.LogWarning("Tried to edit word while another word is already being edited!");
                return;
            }

            _activeInputField = Instantiate(inputFieldPrefab, transform);
            var inputX = IsChord ? 0 : word.BeatWidth / 2f * beatSpacing;
            _activeInputField.transform.localPosition = new Vector3(inputX,-30f,0);
            _activeInputField.onSubmit.AddListener(OnSubmitWordType);
            _activeInputField.SetTextWithoutNotify(word.Text);
            _activeInputField.ActivateInputField();
            
            _activeInputField.onValueChanged.AddListener(s =>
            {
                if (Keyboard.current.shiftKey.isPressed && s.EndsWith(" "))
                    _activeInputField.SetTextWithoutNotify(s.Substring(0, s.Length - 1) + C.CustomSpaceChar);
            });
            
            _inputting = true;
            EditorInputManager.Instance.enabled = false;
        }

        // Handlers
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_dragging || _inputting)
                return;
            
            if (Keyboard.current.ctrlKey.isPressed || Keyboard.current.shiftKey.isPressed || Keyboard.current.altKey.isPressed)
                Edit();
            else
                EditorTrackClipboard.Instance.SelectWord(!Selected ? this : null);
        }

        public void OnSubmitWordType(string result)
        {
            Destroy(_activeInputField.gameObject);
            _activeInputField = null;
            _inputting = false;
            EditorInputManager.Instance.enabled = true;

            if (result == word.Text)
                return;

            if (result.Length == 0)
                word.Delete();
            else
                word.Text = result;

            EditorHistory.Record(word.Beat);
            EditorTrack.Instance.RefreshBeatmap();
        }

        public void Delete()
        {
            word.Delete();
            EditorHistory.Record(word.Beat + word.BeatWidth/2f);
            EditorTrack.Instance.RefreshBeatmap();
        }

        private float DragXToBeatInterval(float dragDelta)
        {
            var index = Mathf.RoundToInt(Mathf.Clamp(_beatIntervalIndexBeforeDrag + dragDelta * DragXToIndexShift,
                0, C.BeatIntervalValues.Count-1));
            return C.BeatIntervalValues[index];
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;
            
            _dragDelta += eventData.delta.x * ScreenToCanvas.Factor;
            if (_dragType == DragType.MoveWord)
                transform.localPosition = new Vector3(_xBeforeDrag + _dragDelta.RoundToNearest(beatSpacing*EditorUtils.GetEditorBeatSnap()), 0, 0);
            else if (!IsChord)
                UpdateBeatInterval(DragXToBeatInterval(_dragDelta));
                
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;
            
            _dragging = false;

            var newBeat = (transform.localPosition.x / beatSpacing).RoundToNearest(EditorUtils.GetEditorBeatSnap());
            if (Math.Abs(word.Beat - newBeat) < C.FloatTolerance &&
                Math.Abs(word.BeatInterval - C.BeatIntervalValues[_beatIntervalIndexBeforeDrag]) < C.FloatTolerance)
                return;

            var oldBeat = word.Beat;
            word.Beat = newBeat;
            EditorHistory.Record((oldBeat + word.Beat) / 2f);
            EditorTrack.Instance.RefreshBeatmap();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_inputting)
                return;
            
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _dragType = DragType.MoveWord;
                _xBeforeDrag = transform.localPosition.x;
            }
            else if (eventData.button == PointerEventData.InputButton.Right && !IsChord)
            {
                _dragType = DragType.ChangeBeatInterval;
                _beatIntervalIndexBeforeDrag = C.BeatIntervalValues.FindIndex(val => val.Equals(word.BeatInterval));
                if (_beatIntervalIndexBeforeDrag == -1)
                    _beatIntervalIndexBeforeDrag = 3;
            }
            else
                return;
            
            _dragDelta = 0f;
            _dragging = true;
        }
    }
}