using System;
using UnityEngine;

namespace Tools.Commons
{
    /// <summary>
    /// Inherit from this base class to create a singleton.
    /// e.g. public class MyClassName : Singleton<MyClassName> {}
    /// Source: http://wiki.unity3d.com/index.php/Singleton
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // Check to see if we're about to be destroyed.
        private static bool _shuttingDown = false;
        private static object _lock = new object();
        private static T _instance;
 
        /// <summary>
        /// Access singleton instance through this propriety.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_shuttingDown)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                                     "' already destroyed. Returning null.");
                    return null;
                }
 
                lock (_lock)
                {
                    if (_instance)
                        return _instance;
                
                    // Search for existing instance.
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (_instance)
                        return _instance;
                
                    // Create new instance if one doesn't already exist.
                    // Need to create a new GameObject to attach the singleton to.
                    var singletonObject = new GameObject();
                    _instance = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T) + " (Singleton)";

                    // Make instance persistent.
                    DontDestroyOnLoad(singletonObject);

                    return _instance;
                }
            }
        }

        protected void OnApplicationQuit()
        {
            _shuttingDown = true;
        }

        protected void OnDestroy()
        {
            _instance = null;
        }
    }
}