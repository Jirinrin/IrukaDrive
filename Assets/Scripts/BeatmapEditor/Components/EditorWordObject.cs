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
        private DragType _dragType;
        private float _xBeforeDrag;
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
                SetColor(_selected ? Color.green : Color.white);
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
            _activeInputField = Instantiate(inputFieldPrefab, transform);
            var inputX = IsChord ? 0 : word.BeatWidth / 2f * _beatSpacing;
            _activeInputField.transform.localPosition = new Vector3(inputX,-30f,0);
            _activeInputField.onSubmit.AddListener(OnSubmitWordType);
            _activeInputField.SetTextWithoutNotify(word.Text);
            _activeInputField.ActivateInputField();
            
            _inputting = true;
            EditorTrackGestures.Instance.enabled = false;
        }

        // Handlers
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_dragging || _inputting)
                return;
            
            if (Keyboard.current.altKey.isPressed)
                EditorTrackClipboard.Instance.SelectWord(!Selected ? this : null);
            else
                Edit();
        }

        public void OnSubmitWordType(string result)
        {
            if (result.Length == 0)
                word.Delete();
            else
                word.Text = result;
            
            EditorTrack.Instance.RefreshBeatmap();
            Destroy(_activeInputField.gameObject);
            
            _inputting = false;
            EditorTrackGestures.Instance.enabled = true;
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
                transform.localPosition = new Vector3(_xBeforeDrag + _dragDelta.RoundToNearest(_beatSpacing*C.EditorBeatSnap), 0, 0);
            else if (!IsChord)
                UpdateBeatInterval(DragXToBeatInterval(_dragDelta));
                
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;
            
            word.Beat = (transform.localPosition.x / _beatSpacing).RoundToNearest(C.EditorBeatSnap);
            EditorTrack.Instance.RefreshBeatmap();
            _dragging = false;
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