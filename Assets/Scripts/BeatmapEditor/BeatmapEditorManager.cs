using BeatmapEditor.SingletonComponents;
using Gameplay;
using Shared;
using Shared.Domain;
using Tools;
using Tools.Commons;

namespace BeatmapEditor
{
    public class BeatmapEditorManager : Singleton<BeatmapEditorManager>
    {
        public void PlayBeatmapFrom(float time)
        {
            // todo: capture all relevant state about the currently editing beatmap and current pan and zoom things and stuff in a static field
            // todo: also set a static field indicating to 'pick up where you left off' instead of having to manually choose a beatmap file to load and stuff
            // todo: call GameManager ToBeatmap with some args and stuff, and set 
        }
        
        private void Start()
        {
            // Uncomment this for easy iterating
            // currentBeatmap = SerializationHelpers.LoadBeatmap(Environment.ExpandEnvironmentVariables(
            //     @"%USERPROFILE%\Documents\Unity\IrukaDrive\Assets\Beatmaps\Tutorial\bla3.blarr"));
            // EditorTrack.Instance.InitTrack(currentBeatmap);
        }

        public void SaveBeatmap()
        {
            SerializationHelpers.SaveBeatmap(currentBeatmap);
            // todo: display some message that it succeeded
        }
        public void SaveBeatmapAs()
        {
            // todo: return the new file name so the beatmapEditor can continue to work with that
            SerializationHelpers.SaveBeatmapAs(currentBeatmap);
            // todo: display some message that it succeeded
        }
        
        public void LoadBeatmap()
        {
            currentBeatmap = SerializationHelpers.LoadBeatmap() ?? currentBeatmap;
            EditorTrack.Instance.InitTrack(currentBeatmap);
        }

        public void BackToMainMenu() => GameManager.ToMainMenu();
    }
}
