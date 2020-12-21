using Gameplay;
using Tools.Commons;
using UnityEngine;

namespace Shared
{
    [RequireComponent(typeof(AudioSource))]
    public class SfxManager : Singleton<SfxManager>
    {
        [SerializeField] private AudioClip tickSample = null;
    
        private AudioSource _audioSource;
    
        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void Tap(char character) => _audioSource.PlayOneShot(tickSample);

        public void MakeTickSound() => _audioSource.PlayOneShot(tickSample);

        private void OnEnable()
        {
            PlayerInputManager.OnChar += Tap;
        }
        private void OnDisable()
        {
            PlayerInputManager.OnChar -= Tap;
        }
    }
}
