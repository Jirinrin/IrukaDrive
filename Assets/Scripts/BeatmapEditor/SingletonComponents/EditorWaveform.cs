using System.Collections.Generic;
using System.Linq;
using Shapes;
using Shared.Domain;
using Tools.Commons;
using UnityEngine;

namespace BeatmapEditor.SingletonComponents
{
    public class EditorWaveform : Singleton<EditorWaveform>
    {
        private const int EveryHowManySamples = 300;

        private bool _active;

        private List<float> _samples;
        private float _beatsPerSample;
        private Song _currentSong;

        public void LoadSong(Song song)
        {
            // todo: something if mp3?? (Because then the samples array simply gets filled with 0s) (or we can just ignore mp3 haha, this is a bonus feature after all)
            if (!song.audio || song.audioPath.EndsWith(".mp3"))
            {
                _active = false;
                return;
            }

            _currentSong = song;

            // it's important to multiply by the number of channels!
            var samplesPerBeat = (song.audio.samples * song.audio.channels) / (song.audio.length * song.BeatsPerSec);
            _beatsPerSample = 1f / samplesPerBeat;

            _active = true;

            var samples = new float[song.audio.samples * song.audio.channels];
            song.audio.GetData(samples, 0);

            _samples = samples.Where((s, i) => i % EveryHowManySamples == 0).ToList();
        }

        private void DrawWaveform(Camera cam)
        {
            if (!_active)
                return;

            Draw.Matrix = transform.localToWorldMatrix;
            Draw.BlendMode = ShapesBlendMode.Transparent;
            Draw.LineGeometry = LineGeometry.Flat2D;
            Draw.LineThickness = .02f;
            Draw.Color = new Color(.3f,.3f,.3f);
            Draw.PolylineJoins = PolylineJoins.Round;

            var xOffset = _currentSong.beatOffset * _currentSong.BeatsPerSec * EditorTrack.viewState.beatSpacing * -1;

            var xPerSample = _beatsPerSample * EditorTrack.viewState.beatSpacing * EveryHowManySamples;
            var samplesPerX = 1f / xPerSample;
            var sampleOffset = Mathf.FloorToInt(Mathf.Max(samplesPerX * (EditorTrack.viewState.panX - xOffset), 0));
            var numberOfSamples = Mathf.FloorToInt(Mathf.Min(samplesPerX * EditorTrack.Instance.containerRect.width, Mathf.Max(_samples.Count - sampleOffset, 0f)));

            using var p = new PolylinePath();
            var points = _samples.GetRange(sampleOffset, numberOfSamples).Select((s, i) => new PolylinePoint(new Vector2(xPerSample*(i+sampleOffset) + xOffset, s * 30f)));
            p.AddPoints(points);
            Draw.Polyline(p);
        }

        private void OnEnable() => Camera.onPostRender += DrawWaveform;
        private void OnDisable() => Camera.onPostRender -= DrawWaveform;
    }
}