
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatmapEditor
{
    public class EditorTrackScrubber : MonoBehaviour, IDragHandler
    {
        public void OnDrag(PointerEventData eventData)
        {
            EditorTrack.Instance.Zoom(eventData.delta.x / 10f);
        }
    }
}
