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

        private static readonly LinkedList<Beatmap> HistoryStack = new LinkedList<Beatmap>();
        private static readonly LinkedList<Beatmap> RedoStack = new LinkedList<Beatmap>();

        public static void Reset()
        {
            HistoryStack.Clear();
            Record();
        }

        public static void Record()
        {
            RedoStack.Clear();
            WriteStateToHistory();
        }

        private static void WriteStateToHistory()
        {
            // todo: async?
            HistoryStack.AddFirst(BeatmapEditorManager.currentBeatmap.CloneState());
            // Debug.Log($"Write state to history => {HistoryStack.Count}; {BeatmapEditorManager.currentBeatmap.NotesCount}");
            if (HistoryStack.Count > MAX_HISTORY_STATES)
                HistoryStack.RemoveLast();
        }

        public static void Undo()
        {
            if (HistoryStack.Count <= 1) return;

            RedoStack.AddFirst(HistoryStack.First());
            HistoryStack.RemoveFirst();
            BeatmapEditorManager.currentBeatmap = HistoryStack.First().CloneState();
            EditorTrack.Instance.RefreshBeatmap(false);
            // Debug.Log($"Undo => {HistoryStack.Count}; {BeatmapEditorManager.currentBeatmap.NotesCount}");
        }

        public static void Redo()
        {
            if (RedoStack.Count < 1) return;
            BeatmapEditorManager.currentBeatmap = RedoStack.First();
            RedoStack.RemoveFirst();
            EditorTrack.Instance.RefreshBeatmap(false);
            WriteStateToHistory();
            // Debug.Log($"Redo => {RedoStack.Count}; {BeatmapEditorManager.currentBeatmap.NotesCount}");
        }
    }
}