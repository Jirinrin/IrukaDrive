using System;
using TMPro;
using UnityEngine;

public class CharacterHitAnim : MonoBehaviour
{
    [NonSerialized] public TextMeshProUGUI text;
    private Animation _animation;
    
    public void AnimationFinished()
    {
        gameObject.SetActive(false);
        OnFinish?.Invoke();
    }

    public void Awake()
    {
        gameObject.SetActive(false);
        text = GetComponentInChildren<TextMeshProUGUI>();
        _animation = GetComponent<Animation>();
    }

    public void StartAnim()
    {
        gameObject.SetActive(true);
        _animation.Play();
    }

    public Action OnFinish;
}