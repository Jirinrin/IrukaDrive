using Shapes;
using Shared.Domain;
using Tools;
using Tools.Commons;
using UnityEngine;

namespace BeatmapEditor.SingletonComponents
{
    [RequireComponent(typeof(Line))]
    public class EditorEndMark : Singleton<EditorEndMark>
    {
        private bool _initted;
        private Line _line;
        private Quad _quad;

        public void Init()
        {
            if (_initted)
            {
                Debug.LogWarning("Editor End Mark already initted");
                return;
            }

            var lineY = EditorTrack.Instance.containerRect.height / 2f - 2f;
            _line = GetComponent<Line>();
            _line.Start = new Vector3(0, lineY, 0);
            _line.End = new Vector3(0, -lineY, 0);
            _quad = GetComponentInChildren<Quad>();

            var quadY = lineY + 1f;
            _quad.A = new Vector3(0, quadY, 0);
            _quad.B = new Vector3(50, quadY, 0);
            _quad.C = new Vector3(50, -quadY, 0);
            _quad.D = new Vector3(0, -quadY, 0);
        }

        public async void UpdatePos()
        {
            var b = BeatmapEditorManager.currentBeatmap;
            var length = b.finishTimestamp ?? (await b.song.Audio)?.length;
            if (length == null)
                return;

            transform.localPosition = new Vector3(EditorTrack.viewState.beatSpacing * b.song.SecToBeats(length.Value), 0, 0);
        }
    }
}
