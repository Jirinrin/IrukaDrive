using System;
using Shared;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatmapEditor
{
    public class EditorWordObject : WordObject<EditorCharObject, EditorWord, ParsedNote>, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        [NonSerialized] public TMP_InputField InputFieldPrefab;
        
        private float _dragDelta;
        private bool _dragging;
        private DragType _dragType;
        private float _xBeforeDrag;
        private int _beatIntervalIndexBeforeDrag;

        private TMP_InputField _activeInputField;
        private static bool _inputting;

        // Constants and stuff
        
        private const float DragXToIndexShift = .1f;

        private enum DragType
        {
            MoveWord,
            ChangeBeatInterval,
        }

        private void RefreshWord()
        {
            foreach (var charObj in charObjRefs)
                charObj.transform.localPosition = new Vector3(_beatSpacing * charObj.Note.Beat, 0, 0);
        }

        private float _beatSpacing;
        public void UpdateSpacing(float newSpacing)
        {
            _beatSpacing = newSpacing;
            RefreshWord();
        }

        private void UpdateBeatInterval(float newInterval)
        {
            Word.BeatInterval = newInterval;
            RefreshWord();
        }

        public void Edit()
        {
            _activeInputField = Instantiate(InputFieldPrefab, transform);
            _activeInputField.transform.localPosition = new Vector3(Word.BeatWidth/2f * _beatSpacing,-30f,0);
            _activeInputField.onSubmit.AddListener(OnSubmitWordType);
            _activeInputField.SetTextWithoutNotify(Word.Text);
            _activeInputField.ActivateInputField();
            
            _inputting = true;
            EditorTrackGestures.Instance.enabled = false;
        }

        // Handlers
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_dragging || _inputting)
                return;
            
            Edit();
        }

        public void OnSubmitWordType(string result)
        {
            if (result.Length == 0)
                Word.Delete();
            else
                Word.Text = result;
            
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
            else
                UpdateBeatInterval(DragXToBeatInterval(_dragDelta));
                
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;
            
            Word.Beat = (transform.localPosition.x / _beatSpacing).RoundToNearest(C.EditorBeatSnap);
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
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                _dragType = DragType.ChangeBeatInterval;
                _beatIntervalIndexBeforeDrag = C.BeatIntervalValues.FindIndex(val => val.Equals(Word.BeatInterval));
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