using System;
using Tools.Commons;
using UnityEngine.InputSystem;

namespace Menu
{
    public class MenuInputManager : Singleton<MenuInputManager>
    {
        public static event Action<int> PressVert;
        public static event Action<int> PressVertSkip;
        public static event Action<int> PressVertExtreme;
        public static event Action<int> PressHor;

        public void OnVert(InputValue v) => PressVert?.Invoke((int) v.Get<float>());
        public void OnVertSkip(InputValue v) => PressVertSkip?.Invoke((int) v.Get<float>());
        public void OnVertExtreme(InputValue v) => PressVertExtreme?.Invoke((int) v.Get<float>());
        public void OnHor(InputValue v) => PressHor?.Invoke((int) v.Get<float>());
    }
}