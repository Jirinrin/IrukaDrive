using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared.Domain
{
    [Serializable]
    public class GameSettings
    {
        public float beatmapScrollSpeedMod = 1f;
    }

    [Serializable]
    public class PlayerScores
    {
        public Dictionary<Guid, SortedSet<BeatmapScore>> scores = new Dictionary<Guid, SortedSet<BeatmapScore>>();
        
        public void AddScore(Guid beatmapId, BeatmapScore score)
        {
            if (!scores.ContainsKey(beatmapId))
                scores[beatmapId] = new SortedSet<BeatmapScore>();
            scores[beatmapId].Add(score);
        }

        public override string ToString()
        {
            return string.Join(", ", scores.Select(score =>
                $"[{score.Key} - {string.Join(", ", score.Value.Select(item => item))}]"
            ));
        }
    }
}