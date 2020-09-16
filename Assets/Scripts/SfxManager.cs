using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : MonoBehaviour
{
    [SerializeField] private AudioClip tickSample = null;
    
    private AudioSource _audioSource;
    
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Tap(char character)
    {
        _audioSource.PlayOneShot(tickSample);
    }

    private void OnEnable()
    {
        PlayerInputManager.OnChar += Tap;
    }
}
