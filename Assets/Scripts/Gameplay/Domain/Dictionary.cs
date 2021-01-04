using System;
using System.IO;
using Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Domain
{
    [Serializable]
    public class Dict
    {
        private static Dict _dictEn;
        public static Dict DictEn
        {
            get
            {
                if (_dictEn != null) return _dictEn;
                _dictEn = Serialization.ReadFromXmlFile<Dict>(Path.Combine(Application.streamingAssetsPath, "Dict/dict_en.xml"));
                return _dictEn;
            }
        }

        // Max length 18 probably
        public string[][] words;

        public string GetRandomWordOfLength(int length)
        {
            var w = words[length];
            return w[Random.Range(0, w.Length - 1)];
        }
    }
}