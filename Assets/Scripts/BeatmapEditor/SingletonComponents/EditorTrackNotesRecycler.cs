using System.Collections.Generic;
using System.Linq;
using BeatmapEditor.Components;
using BeatmapEditor.Domain;
using Shared;
using Shared.Domain;
using TMPro;
using UnityEngine;

namespace BeatmapEditor.SingletonComponents
{
    public class EditorTrackNotesRecycler : TrackNotesRecyclerBase<EditorTrackNotesRecycler, EditorWord, ParsedChar, EditorWordObject>
    {
        [SerializeField] private TMP_InputField inputFieldPrefab = null;

        private Beatmap _currentBeatmap;

        protected override void InitWord(ObjWidthItem item)
        {
            item.obj.inputFieldPrefab = inputFieldPrefab;
            base.InitWord(item);
        }

        protected override void CleanupWord(EditorWordObject item, int index)
        {
            item.Selected = false;
            base.CleanupWord(item, index);
        }

        public void LoadBeatmap(List<BeatmapWord> words) =>
            base.LoadBeatmap(words.Select(word => new EditorWord(word)));

        public void Init(Beatmap beatmap)
        {
            containerWidth = EditorTrack.Instance.containerRect.width;
            _currentBeatmap = beatmap;
            LoadBeatmap(_currentBeatmap.words);
        }

        public void RefreshBeatmap() => LoadNewWords(_currentBeatmap.words.Select(word => new EditorWord(word)));

        public void EditWord(int index) =>
            wordRecyclerList.visibleItemsLookup[index].obj.Edit();

        protected override float PanX => EditorTrack.viewState.panX;
        protected override float BeatSpacing => EditorTrack.viewState.beatSpacing;
    }
}