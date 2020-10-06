using System;
using Menu.ScreenControllers.SettingControllers;
using Shared;
using UnityEngine;

namespace Menu.ScreenControllers
{
    public class SettingsScreen : MonoBehaviour
    {
        [SerializeField] private GameObject contentContainer;
        [SerializeField] private FloatSetting floatSetting;
        
        private void Start()
        {
            // var setting1 = Instantiate(floatSetting, contentContainer.transform);
            // contentContainer.
            // var test = PlayerPrefs.
        }

        public void BackToMainMenu()
        {
            Local.CommitSettings();
            MenuManager.Instance.ToScreen(MenuScreen.Title);
        }
    }
}
