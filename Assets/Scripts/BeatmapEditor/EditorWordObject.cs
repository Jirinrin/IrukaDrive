using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatmapEditor
{
    public class EditorWordObject : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        public EditorWord Word;

        [CanBeNull] public List<EditorCharObject> CharObjRefs;

        [NonSerialized] public TMP_InputField InputFieldPrefab;
        
        private float xBeforeDrag;
        private float _dragDelta;
        private bool _dragging;

        private TMP_InputField _activeInputField;
        private static bool _inputting;

        private const float SnapModulus = .5f;

        private float _beatSpacing;
        public void UpdateSpacing(float newSpacing)
        {
            _beatSpacing = newSpacing;
            foreach (var charObj in CharObjRefs)
                charObj.transform.localPosition = new Vector3(_beatSpacing * charObj.Note.Beat, 0, 0);
        }
        
        // Handlers
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_dragging || _inputting)
                return;

            _activeInputField = Instantiate(InputFieldPrefab, transform);
            _activeInputField.transform.localPosition = new Vector3(Word.BeatWidth/2f * _beatSpacing,-30f,0);
            _activeInputField.onSubmit.AddListener(OnSubmitWordType);
            _activeInputField.ActivateInputField();
            
            _inputting = true;
            EditorTrackGestures.Instance.enabled = false;
        }

        public void OnSubmitWordType(string result)
        {
            Word.Text = result;
            EditorTrack.Instance.RefreshBeatmap();
            Destroy(_activeInputField.gameObject);
            
            _inputting = false;
            EditorTrackGestures.Instance.enabled = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;
            
            _dragDelta += eventData.delta.x * ScreenToCanvas.Factor;
            transform.localPosition = new Vector3(xBeforeDrag + _dragDelta.RoundToNearest(_beatSpacing*SnapModulus), 0, 0);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;
            
            Word.Beat = (transform.localPosition.x / _beatSpacing).RoundToNearest(SnapModulus);
            EditorTrack.Instance.RefreshBeatmap();
            _dragging = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_inputting)
                return;
            
            // todo: figure out a way to make this event go through to parents (i.e. EditorTrack)
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            
            _dragDelta = 0f;
            xBeforeDrag = transform.localPosition.x;
            _dragging = true;
        }
    }
}