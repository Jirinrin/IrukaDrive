using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatmapEditor
{
    public class EditorWordObject : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        public EditorWord Word;

        [CanBeNull] public List<EditorCharObject> CharObjRefs;
        
        private float xBeforeDrag;
        private float _dragDelta;

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
            Debug.Log("pointer click");
        }

        public void OnDrag(PointerEventData eventData)
        {
            _dragDelta += eventData.delta.x;
            transform.localPosition = new Vector3(xBeforeDrag + _dragDelta.RoundToNearest(_beatSpacing*SnapModulus), 0, 0);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Word.Beat = (transform.localPosition.x / _beatSpacing).RoundToNearest(SnapModulus);
            EditorTrack.Instance.RefreshBeatmap();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragDelta = 0f;
            xBeforeDrag = transform.localPosition.x;
        }
    }
}