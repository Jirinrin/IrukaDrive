
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatmapEditor
{
    public class EditorTrackGestures : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        private Vector2 _beginDragPoint;
        
        public void OnDrag(PointerEventData eventData)
        {
            EditorTrack.Instance.Pan(eventData.delta.x * ScreenToCanvas.Factor);
            EditorTrack.Instance.Zoom(eventData.delta.y / 60f, eventData.position.x * ScreenToCanvas.Factor);
        }

        private void OnMouseEnter()
        {
            // todo: implement with proper textures, also OnMouseExit
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _beginDragPoint = eventData.position;
        }
    }
}
