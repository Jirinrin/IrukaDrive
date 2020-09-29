using System.Collections.Generic;
using System.Linq;
using Gameplay; // todo: remove
using Shared;
using TMPro;
using Tools.Commons;
using UnityEngine;

namespace BeatmapEditor
{
    // todo: better 'shared container / coordinate' system to sync to EditorTrack
    public class EditorTrackNotesRecycler : TrackNotesRecyclerBase<EditorTrackNotesRecycler, EditorWord, ParsedNote, EditorCharObject, EditorWordObject>
    {
        [SerializeField] private TMP_InputField inputFieldPrefab = null;

        private Beatmap _currentBeatmap;

        protected override void InitCharObj(EditorCharObject charObj, ParsedNote note)
        {
            charObj.Init(note);
        }

        protected override void CleanupWord(EditorWordObject item, int index)
        {
            foreach (var charObject in item.charObjRefs)
                charObject.Cleanup();
            base.CleanupWord(item, index);
        }

        private void UpdateSpacing()
        {
            var lookup = wordRecyclerList.visibleItemsLookup;
            foreach (var index in lookup.Keys)
            {
                lookup[index].transform.localPosition = new Vector3(beatSpacing * index.IndexToBeat(), 0, 0);
                lookup[index].UpdateSpacing(beatSpacing);
            }
        }

        public void LoadBeatmap(List<BeatmapWord> words)
        {
            base.LoadBeatmap(words.Select(word => new EditorWord(word)));
        }

        public void Init(Beatmap beatmap)
        {
            containerWidth = EditorTrack.Instance.containerRect.width;
            _currentBeatmap = beatmap;
            LoadBeatmap(_currentBeatmap.words);
            base.Init();
        }

        public void RefreshBeatmap()
        {
            LoadBeatmap(_currentBeatmap.words);
            wordRecyclerList.Refresh();
        }

        public override void RefreshWindow()
        {
            base.RefreshWindow();
            UpdateSpacing();
        }

        public void EditWord(int index) =>
            wordRecyclerList.visibleItemsLookup[index].Edit();

        // Coming from EditorTrack
        
        private void OnZoom(float beatSpacing) => base.beatSpacing = beatSpacing;
        private void OnEnable()
        {
            EditorTrack.OnPan += OnPan;
            EditorTrack.OnZoom += OnZoom;
        }
        private void OnDisable()
        {
            EditorTrack.OnPan -= OnPan;
            EditorTrack.OnZoom -= OnZoom;
        }
    }
}