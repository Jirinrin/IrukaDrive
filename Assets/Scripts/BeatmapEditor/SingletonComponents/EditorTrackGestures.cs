using BeatmapEditor.Domain;
using Shared;
using Tools;
using Tools.Commons;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace BeatmapEditor.SingletonComponents
{
    // todo: use double-click for certain things
    public class EditorTrackGestures : Singleton<EditorTrackGestures>, IDragHandler, IBeginDragHandler, IScrollHandler, IPointerClickHandler
    {
        private Vector2 _beginDragPoint;
        
        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right || eventData.button == PointerEventData.InputButton.Middle)
            {
                if (Keyboard.current.shiftKey.isPressed)
                    EditorTrack.Instance.Zoom(eventData.delta.y / 60f, eventData.position.x * ScreenToCanvas.Factor);
                else
                {
                    var xFactor = Keyboard.current.altKey.isPressed ? 4f : 1f;
                    EditorTrack.Instance.Pan(eventData.delta.x * ScreenToCanvas.Factor * xFactor);
                }
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

        public void OnPointerClick(PointerEventData eventData)
        {
            var x = eventData.position.x * ScreenToCanvas.Factor;
            if (Keyboard.current.ctrlKey.isPressed)
                EditorTrack.Instance.CreateWord(x);
            else if (eventData.button == PointerEventData.InputButton.Left)
                EditorTrackClipboard.Instance.SetCursor(x);
        }

        private void OnPlay() => 
            EditorTrack.Instance.PlayFromPoint(Input.mousePosition.x * ScreenToCanvas.Factor, !Keyboard.current.shiftKey.isPressed);

        private void OnCopy() => EditorTrackClipboard.Instance.Copy();
        private void OnPaste() => EditorTrackClipboard.Instance.Paste();

        private void OnEnable()
        {
            InputManager.PressPlay += OnPlay;
            EditorInputManager.Copy += OnCopy;
            EditorInputManager.Paste += OnPaste;
        }
        private void OnDisable()
        {
            InputManager.PressPlay -= OnPlay;
            EditorInputManager.Copy -= OnCopy;
            EditorInputManager.Paste -= OnPaste;
        }
    }
}
