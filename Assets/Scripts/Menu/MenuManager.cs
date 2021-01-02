using System;
using System.Collections.Generic;
using Shared;
using Tools.Commons;
using UnityEngine;

namespace Menu
{
    public enum MenuScreen
    {
        Title,
        Settings,
        SongSelect,
        Result
    }
    
    public class MenuManager : Singleton<MenuManager>
    {
        // Init params for MenuScreen
        public MenuScreen CurrentScreen { get; private set; } = GameManager.InitMenuScreen;

        private readonly Dictionary<string, GameObject> _screens = new Dictionary<string, GameObject>();
        private GameObject CurrentScreenObj => _screens[CurrentScreen.ToString()];

        private void Awake()
        {
            var screens = GetComponentsInChildren<Canvas>();

            foreach (var screen in screens)
            {
                var obj = screen.gameObject;
                _screens[obj.name] = obj;
                obj.SetActive(false);
            }
            _screens[CurrentScreen.ToString()].gameObject.SetActive(true);
        }

        public void ToScreen(string screen)
        {
            ToScreen((MenuScreen) Enum.Parse(typeof(MenuScreen), screen));
        }

        public void ToScreen(MenuScreen screen)
        {
            // todo: fancier animation
            CurrentScreenObj.SetActive(false);
            CurrentScreen = screen;
            CurrentScreenObj.SetActive(true);
        }

        public void ToEditor() =>
            GameManager.ToBeatmapEditor();

        public void Exit() => GameManager.Exit();
    }
}