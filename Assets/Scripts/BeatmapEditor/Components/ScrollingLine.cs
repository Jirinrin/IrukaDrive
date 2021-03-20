using Shapes;
using UnityEngine;

namespace BeatmapEditor.Components
{
    [RequireComponent(typeof(Line))]
    public class ScrollingLine : MonoBehaviour
    {
        [SerializeField] private float scrollSpeed = 1;

        private Line _line;

        private void OnEnable() => _line = GetComponent<Line>();

        private void Update() => _line.DashOffset = Time.time * scrollSpeed;
    }
}
