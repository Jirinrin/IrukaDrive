using System.IO;
using Shared.Domain;
using Tools;
using Tools.Commons;
using UnityEngine;

namespace Shared
{
    // todo: periodically do checks that no scores of non-existing beatmaps exist, no non-existing settings etc
    // Name is more like LocalStorageManager but this way access is more readable
    public class Local : Singleton<Local>
    {
        private static bool _init = true;
        
        public static GameSettings Settings { get; private set; }
        public static PlayerScores Scores { get; private set; }

        private static string _settingsPath;
        private static string _scoresPath;

        private void Awake()
        {
            if (_init)
            {
                _init = false;
                Init();
            }
        }

        private static void Init()
        {
            _settingsPath = Path.Combine(Application.persistentDataPath, "settings.xml");
            _scoresPath = Path.Combine(Application.persistentDataPath, "scores.db");
                
            // todo: settings json instead of xml
            Settings = File.Exists(_settingsPath) ? Serialization.ReadFromXmlFile<GameSettings>(_settingsPath) : new GameSettings();
            Scores = File.Exists(_scoresPath) ? Serialization.ReadFromBinaryFile<PlayerScores>(_scoresPath) : new PlayerScores();
        }

        public static void CommitSettings()
        {
            Serialization.WriteToXmlFile(_settingsPath, Settings);
        }

        // todo: make async or sth
        public static void CommitScores()
        {
            Serialization.WriteToBinaryFile(_scoresPath, Scores);
        }
    }
}