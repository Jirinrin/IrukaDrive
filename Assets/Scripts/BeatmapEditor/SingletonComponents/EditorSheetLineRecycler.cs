using System.Collections.Generic;
using BeatmapEditor.Domain;
using Shapes;
using Shared;
using Shared.Domain;
using Tools.Commons;
using UnityEngine;

namespace BeatmapEditor.SingletonComponents
{
    public class EditorSheetLineRecycler : SheetLineRecyclerBase
    {
        public override void Init(Beatmap beatmap)
        {
            containerRect = EditorTrack.Instance.containerRect;
            base.Init(beatmap);
        }
        
        private void OnEnable()
        {
            EditorTrackViewState.OnPan += OnPan;
            EditorTrackViewState.OnZoom += OnZoom;
        }
        private void OnDisable()
        {
            EditorTrackViewState.OnPan -= OnPan;
            EditorTrackViewState.OnZoom -= OnZoom;
        }
    }
}