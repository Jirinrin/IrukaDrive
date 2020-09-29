using UnityEngine;

namespace Tools
{
    public class ScreenToCanvas : MonoBehaviour
    {
        private static bool _initialized;
        private static float _screenToCanvasFactor;
        public static float Factor
        {
            get
            {
                if (_initialized)
                    return _screenToCanvasFactor;

                var canvas = (Canvas) FindObjectOfType(typeof(Canvas));
                if (canvas == null)
                {
                    Debug.LogError("Could not find Canvas");
                    return 0f;
                }

                var canvasRectTransform = canvas.GetComponent<RectTransform>();

                _screenToCanvasFactor = canvasRectTransform.rect.width / Screen.width;
                _initialized = true;
                
                return _screenToCanvasFactor;
            }
        } 
    }
}
