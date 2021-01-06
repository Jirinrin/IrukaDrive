using Shared;
using UnityEngine;

namespace Menu.ScreenControllers
{
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer wheel;
        [SerializeField] private GameObject irukaWrapper;

        [SerializeField] private float wheelRotateSpeed;
        [SerializeField] private float irukaBobAmp;
        [SerializeField] private float irukaBobSpeed;

        public void Update()
        {
            wheel.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Time.time * wheelRotateSpeed));
            irukaWrapper.transform.rotation = Quaternion.Euler(new Vector3(0, 0, irukaBobAmp * Mathf.Sin(Time.time * irukaBobSpeed)));
        }

        public void Drive() => MenuManager.Instance.ToScreen(MenuScreen.SongSelect);

        private void OnEnable() => InputManager.PressConfirm += Drive;
        private void OnDisable() => InputManager.PressConfirm -= Drive;
    }
}
