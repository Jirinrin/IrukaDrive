using Shared;
using UnityEngine.InputSystem;

namespace BeatmapEditor
{
    public static class EditorUtils
    {
        public static float GetEditorBeatSnap() =>
            Keyboard.current.shiftKey.isPressed ? (Keyboard.current.altKey.isPressed ? C.EditorBeatSnapSuperFine : C.EditorBeatSnapFine) : C.EditorBeatSnap;
    }
}