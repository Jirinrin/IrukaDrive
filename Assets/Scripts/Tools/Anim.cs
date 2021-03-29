using System;
using DG.Tweening;
using UnityEngine;

namespace Tools
{
    public static class Anim
    {
        public static readonly Func<float, float> SmallBlink = tt =>
            Interp.BlinkDownUp(.01f, .05f, tt) * Interp.BlinkDownUp(.09f, .05f, tt);

        public static readonly Func<float, float> BigBlink = tt =>
            Interp.BlinkDownUp(.01f, .05f, tt) * Interp.BlinkDownUp(.13f, .05f, tt) *
            Interp.BlinkDownUp(.35f, .01f, tt);

        public static void DoAnim(float dur, Action<float> fn)
        {
            var t = 0f;
            void Tr(float newT)
            {
                t = newT;
                var tt = t * dur;
                fn(tt);
            }

            DOTween.To(() => t, Tr, 1f, dur);
        }

        // todo: properly use a period in seconds
        public static float Pulsate(float speed, float ampl, float offset = 0f) => (Mathf.Sin((Time.time + offset) * speed) * .5f - .5f) * ampl + 1f;

        public static void Periodic(float dur, Action<float> fn, float offset = 0f) => fn((Time.time + offset) % dur);
        public static void PeriodicNorm(float dur, Action<float> fn, float offset = 0f) => fn((Time.time / dur + offset) % 1);
    }
}