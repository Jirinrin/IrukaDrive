// ReSharper disable InconsistentNaming

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatmapEditor
{
    [RequireComponent(typeof(PlayerInput))]
    public class EditorInputManager : MonoBehaviour
    {
        public static event Action Save;
        public void OnSave() => Save?.Invoke();
        
        public static event Action Copy;
        public void OnCopy() => Copy?.Invoke();
        
        public static event Action Paste;
        public void OnPaste() => Paste?.Invoke();
        
        public static event Action Play;
        public void OnPlay() => Play?.Invoke();
    }
}