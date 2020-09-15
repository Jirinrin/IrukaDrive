using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace BeatmapEditor
{
    public class EditorTrackGestures : MonoBehaviour, IDragHandler, IBeginDragHandler, IScrollHandler
    {
        private Vector2 _beginDragPoint;
        
        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right || eventData.button == PointerEventData.InputButton.Middle)
            {
                if (Keyboard.current.shiftKey.isPressed)
                    EditorTrack.Instance.Zoom(eventData.delta.y / 60f, eventData.position.x * ScreenToCanvas.Factor);
                else
                    EditorTrack.Instance.Pan(eventData.delta.x * ScreenToCanvas.Factor);
            }
        }

        private void OnMouseEnter()
        {
            // todo: implement with proper textures, also OnMouseExit
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _beginDragPoint = eventData.position;
        }

        public void OnScroll(PointerEventData eventData)
        {
            EditorTrack.Instance.Zoom(eventData.scrollDelta.y * 0.15f, eventData.position.x * ScreenToCanvas.Factor);
        }
    }
}
