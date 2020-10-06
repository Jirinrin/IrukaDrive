using System;
using System.Globalization;

namespace Shared.Domain
{
    [Serializable]
    public class BeatmapScore : IComparable<BeatmapScore>
    {
        // todo: make score saving more detailed
        
        // Range 0-1000000 or sth
        public float score;
        
        public int CompareTo(BeatmapScore other) => score.CompareTo(other.score);

        public override string ToString()
        {
            return score.ToString(CultureInfo.CurrentCulture);
        }
    }
}