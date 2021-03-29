using System;
using Shared.Domain;
using Tools.Commons;
using UnityEngine;

namespace Shared
{
    public class TrackBase<T, TViewState> : Singleton<T>
        where T : TrackBase<T, TViewState>
        where TViewState : ViewState, new()
    {
        [SerializeField] protected RectTransform containerRectTransform = null;
        [NonSerialized] public Rect containerRect;

        public static TViewState viewState = new TViewState();

        protected virtual void Init()
        {
            containerRect = containerRectTransform.rect;
        }
    }
}