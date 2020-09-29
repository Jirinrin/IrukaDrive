using BeatmapEditor.SingletonComponents;
using Gameplay;
using Shared.Domain;
using Tools;
using Tools.Commons;

namespace BeatmapEditor
{
    public class BeatmapEditorManager : Singleton<BeatmapEditorManager>
    {
        public Beatmap currentBeatmap;

        private void Start()
        {
            // Uncomment this for easy iterating
            // currentBeatmap = SerializationHelpers.LoadBeatmap(@"C:\Users\侍鈴\Documents\Unity\IrukaDive\Build\bla.blarr");
            // EditorTrack.Instance.InitTrack(currentBeatmap);
        }

        public void SaveBeatmap()
        {
            SerializationHelpers.SaveBeatmap(currentBeatmap);
            // todo: display some message that it succeeded
        }
        public void SaveBeatmapAs()
        {
            SerializationHelpers.SaveBeatmapAs(currentBeatmap);
            // todo: display some message that it succeeded
        }
        
        public void LoadBeatmap()
        {
            currentBeatmap = SerializationHelpers.LoadBeatmap() ?? currentBeatmap;
            EditorTrack.Instance.InitTrack(currentBeatmap);
        }
    }
}
