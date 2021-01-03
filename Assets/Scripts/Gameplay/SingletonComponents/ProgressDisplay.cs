using Shapes;
using Shared;
using UnityEngine;

namespace Gameplay.SingletonComponents
{
    public class ProgressDisplay : MonoBehaviour
    {
        [SerializeField] private Rectangle progressBar;
        [SerializeField] private Rectangle progressBarBase;

        private float _progressMaxLength;

        private void Awake()
        {
            _progressMaxLength = progressBarBase.Width;
        }

        private void Update()
        {
            if (SongManager.Instance.IsPlaying)
            {
                progressBar.Width = 
                    _progressMaxLength * (SongManager.Instance.songPosSec / SongManager.Instance.songLength);
            }
        }
    }
}
