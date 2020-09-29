using System;
using Shared;
using UnityEngine;

namespace Tools
{
    public static class PrimitiveExtensions
    {
        public static float RoundToNearest(this float value, float modulo) => Mathf.Round(value / modulo) * modulo;

        public static bool Equals(this float value, float otherValue) => Math.Abs(value - otherValue) < C.FloatTolerance;
    }
}