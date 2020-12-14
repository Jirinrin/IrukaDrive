using System;
using Shared.Domain;
using UnityEngine;

namespace Menu.ScreenControllers
{
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer wheel;

        [SerializeField] private float wheelRotateSpeed;

        public void Update()
        {
            wheel.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Time.time * wheelRotateSpeed));
        }
    }
}
