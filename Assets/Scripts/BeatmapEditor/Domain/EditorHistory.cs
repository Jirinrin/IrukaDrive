using System.Collections.Generic;
using System.Linq;
using BeatmapEditor.SingletonComponents;
using Shared.Domain;
using UnityEngine;

namespace BeatmapEditor.Domain
{
    // todo: more pro, by recording where something happened, and sending the editor view to that point when undoing etc
    public static class EditorHistory
    {
        private const int MAX_HISTORY_STATES = 20;

        private static readonly LinkedList<(float?, Beatmap)> HistoryStack = new LinkedList<(float?, Beatmap)>();
        private static readonly LinkedList<(float?, Beatmap)> RedoStack = new LinkedList<(float?, Beatmap)>();

        public static void Reset()
        {
            HistoryStack.Clear();
            Record();
        }

        public static void Record(float? beatWhereItHappened = null)
        {
            RedoStack.Clear();
            WriteStateToHistory(beatWhereItHappened);
        }

        private static void WriteStateToHistory(float? beatWhereItHappened)
        {
            // todo: async?
            HistoryStack.AddFirst((beatWhereItHappened, BeatmapEditorManager.currentBeatmap.CloneState()));
            // Debug.Log($"Write state to history => {HistoryStack.Count}; {BeatmapEditorManager.currentBeatmap.NotesCount}");
            if (HistoryStack.Count > MAX_HISTORY_STATES)
                HistoryStack.RemoveLast();
        }

        public static void Undo()
        {
            if (HistoryStack.Count <= 1) return;

            RedoStack.AddFirst(HistoryStack.First());
            HistoryStack.RemoveFirst();
            var (beat, beatmapState) = HistoryStack.First();
            BeatmapEditorManager.currentBeatmap = beatmapState.CloneState();
            EditorTrack.Instance.RefreshBeatmap();
            if (beat != null)
                EditorTrack.Instance.PanToBeat(beat.Value);
            // Debug.Log($"Undo => {HistoryStack.Count}; {BeatmapEditorManager.currentBeatmap.NotesCount}");
        }

        public static void Redo()
        {
            if (RedoStack.Count < 1) return;
            var (beat, beatmapState) = RedoStack.First();
            BeatmapEditorManager.currentBeatmap = beatmapState;
            RedoStack.RemoveFirst();
            EditorTrack.Instance.RefreshBeatmap();
            WriteStateToHistory(beat);
            if (beat != null)
                EditorTrack.Instance.PanToBeat(beat.Value);
            // Debug.Log($"Redo => {RedoStack.Count}; {BeatmapEditorManager.currentBeatmap.NotesCount}");
        }
    }
}