using Gameplay;
using Menu;
using Shared.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shared
{
    public class GameManager : MonoBehaviour
    {
        public static GameState State { get; private set; } = GameState.Menu;

        public static MenuScreen InitMenuScreen { get; private set; } = MenuScreen.Title;

        public static void ToGameplay(Beatmap beatmap, float startTime = 0f)
        {
            if (State == GameState.Gameplay)
                return;
            
            GameplayManager.PrepGameplay(beatmap, startTime, State == GameState.BeatmapEditor);

            State = GameState.Gameplay;
            SceneManager.LoadScene("Gameplay");
        }
        
        public static void EndGameplay()
        {
            if (State != GameState.Gameplay)
                return;
            
            if (GameplayManager.EditorPlay)
                ToState(GameState.BeatmapEditor);
            else
            {
                InitMenuScreen = MenuScreen.Result;
                ToState(GameState.Menu);
            }
        }

        public static void ToBeatmapEditor()
        {
            if (State == GameState.BeatmapEditor)
                return;
            ToState(GameState.BeatmapEditor);
        }

        public static void ToMainMenu()
        {
            InitMenuScreen = MenuScreen.Title;
            ToState(GameState.Menu);
        }

        private static void ToState(GameState state)
        {
            State = state;
            SceneManager.LoadScene(state.ToString());
        }
    }

    public enum GameState
    {
        Menu,
        Gameplay,
        BeatmapEditor,
    }
}
