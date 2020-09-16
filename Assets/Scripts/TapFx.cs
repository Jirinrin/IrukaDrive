using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// todo: make pulse animation fancy
public class TapFx : MonoBehaviour
{
    private GameObject _sprite;
    [SerializeField] private GameObject circlePrefab = null;

    private const float DefaultScale = 1f;
    private const float MaxScale = 2f;
    private float _scale = DefaultScale;

    private void OnEnable()
    {
        PlayerInputManager.OnTap += OnTap;
    }

    private void Start()
    {
        _sprite = Instantiate(circlePrefab, transform);
    }

    private void OnTap()
    {
        _scale = MaxScale;
    }

    private void Update()
    {
        _sprite.transform.localScale = new Vector3(_scale, _scale);
        if (_scale > DefaultScale)
        {
            _scale -= (MaxScale - DefaultScale) / 70;
        }
    }
}
