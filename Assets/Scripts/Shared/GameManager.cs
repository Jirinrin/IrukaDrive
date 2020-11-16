using System;
using Gameplay;
using JetBrains.Annotations;
using Menu;
using Menu.ScreenControllers;
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
        
        public static void EndGameplay([CanBeNull] BeatmapScore beatmapScore)
        {
            if (State != GameState.Gameplay)
                return;

            if (GameplayManager.EditorPlay)
                ToState(GameState.BeatmapEditor);
            else
            {
                ResultsScreen.beatmapScore = beatmapScore;
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

        [Obsolete("Please don't use this outside of development-only scenarios")]
        public static void SetState(GameState val) => State = val;
    }

    public enum GameState
    {
        Menu,
        Gameplay,
        BeatmapEditor,
    }
}
