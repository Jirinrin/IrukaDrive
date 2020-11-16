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
            var scrollSpeedModSetting = Instantiate(floatSetting, contentContainer.transform);
            scrollSpeedModSetting.Init("Scroll Speed", Local.Settings.BeatmapScrollSpeedMod, 0.25f, 0.25f, 4f, f =>
            {
                Local.Settings.BeatmapScrollSpeedMod = f;
                Local.CommitSettings();
            });
        }

        public void BackToMainMenu()
        {
            Local.CommitSettings();
            MenuManager.Instance.ToScreen(MenuScreen.Title);
        }
    }
}
