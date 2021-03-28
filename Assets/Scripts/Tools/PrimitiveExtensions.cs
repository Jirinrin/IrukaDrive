using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Shared;
using UnityEngine;

namespace Tools
{
    public static class PrimitiveExtensions
    {
        public static float RoundToNearest(this float value, float modulo) => Mathf.Round(value / modulo) * modulo;

        public static bool Equals(this float value, float otherValue) => Math.Abs(value - otherValue) < C.FloatTolerance;

        public static string Join<T>(this IEnumerable<T> l, string separator) => string.Join(separator, l);
        
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)       
            => self.Select((item, index) => (item, index));
        
        public static Color SetAlpha(this Color c, float alpha) => new Color(c.r, c.g, c.b, alpha);
        public static Color SetRGB(this Color c, float r, float g, float b) => new Color(r, g, b, c.a);
        public static void Deconstruct(this Color c, out float r, out float g, out float b) { r = c.r; g = c.g; b = c.b; }

        public static Match Match(this string s, string pattern) => Regex.Match(s, pattern);
        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);
        public static string OrNull(this string s) => s.IsNullOrEmpty() ? null : s;

        public static string Capitalize(this string s) => s.Length <= 1 
            ? s.ToUpper() 
            : char.ToUpper(s[0]) + s.Substring(1);

        public static bool IsEmpty<T>(this List<T> l) => l.Count == 0;
    }
}