using System;
using Gameplay.Domain;
using TMPro;
using UnityEngine;

namespace Gameplay.Components
{
    public class CharacterHitAnim : MonoBehaviour
    {
        private static readonly int PlayAnimKey = Animator.StringToHash("PlayAnim");
        
        [NonSerialized] public TextMeshProUGUI text;
        private Animator _anim;
    
        public void AnimationFinished()
        {
            gameObject.SetActive(false);
            onFinish?.Invoke();
        }

        public void Awake()
        {
            gameObject.SetActive(false);
            text = GetComponentInChildren<TextMeshProUGUI>();
            _anim = GetComponent<Animator>();
        }

        public void StartAnim(NoteResult result)
        {
            gameObject.SetActive(true);
            _anim.SetInteger(PlayAnimKey, (int) result);
        }

        public Action onFinish;
    }
}