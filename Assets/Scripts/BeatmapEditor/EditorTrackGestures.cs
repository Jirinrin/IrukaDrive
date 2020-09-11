
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatmapEditor
{
    public class EditorTrackGestures : MonoBehaviour, IDragHandler
    {
        public void OnDrag(PointerEventData eventData)
        {
            EditorTrack.Instance.Pan(eventData.delta.x);
            EditorTrack.Instance.Zoom(eventData.delta.y / 10f);
        }
    }
}
