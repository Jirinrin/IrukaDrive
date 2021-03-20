using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public static class Interp
    {
        public static float LinearStep(float edge0, float edge1, float x) =>
            Mathf.Clamp((x - edge0) / (edge1 - edge0), 0, 1);

        public static float SmoothStep(float edge0, float edge1, float x) {
            var t = LinearStep(edge0, edge1, x);
            return t * t * (3f - 2f * t);
        }
        public static float ToSms(float lnsNumber) => SmoothStep(0, 1, lnsNumber);

        public static float SmootherStep(float edge0, float edge1, float x)
        {
            // Scale, and clamp x to 0..1 range
            var t = LinearStep(edge0, edge1, x);
            // Evaluate polynomial
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }

        public static float EaseInStep(float edge0, float edge1, float x) {
            var t = LinearStep(edge0, edge1, x);
            return t * t;
        }

        public static float EaseOutStep(float edge0, float edge1, float x) =>
            Mathf.Clamp(SmoothStep(edge0-(edge1-edge0), edge1, x) * 2 - 1, 0,1);
        public static float ToEos(float lnsNumber) => EaseOutStep(0, 1, lnsNumber);

        public static float Sms(float edge0, float transitionWidth, float x) =>
            SmoothStep(edge0, edge0+transitionWidth, x);

        public static float SmsSpike(float top, float edgeDistance, float x) =>
            SmoothStep(top-edgeDistance, top, x) - SmoothStep(top, top+edgeDistance, x);

        public static float Lns(float edge0, float transitionWidth, float x) =>
            LinearStep(edge0, edge0+transitionWidth, x);

        public static List<float> SplitLns(float lnsNumber, int[] splitPoints, float x)
        {
            var output = new List<float>();
            for (var i = 0; i < splitPoints.Length-1; i++)
                output.Add(LinearStep(splitPoints[i], splitPoints[i+1], x));
            return output;
        }

        public static int Step(float edge, float x) =>    x >= edge ? 1 : 0;
        public static int StepExcl(float edge, float x) => x > edge ? 1 : 0;

        public static float BlinkUpDown(float up, float duration, float x) =>
            Step(up,x) - Step(up+duration,x);
        public static float BlinkDownUp(float down, float dur, float x) => 1f - BlinkUpDown(down, dur, x);
    }
}