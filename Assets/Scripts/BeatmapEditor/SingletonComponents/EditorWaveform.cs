using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Shapes;
using Shared.Domain;
using UnityEngine;
using UnityEngine.Rendering;

namespace BeatmapEditor.SingletonComponents
{
    public class EditorWaveform : ImmediateModeShapeDrawer
    {
        private const int EveryHowManySamples = 300;

        private bool _active;
        private float _opacity;

        private List<float> _samples;
        private float _beatsPerSample;
        private Song _currentSong;

        public async void LoadSong(Song song)
        {
            if (song == null) return;
            OpacityTransition(0, .2f);
            var clip = await song.Audio;

            // todo: something if mp3?? (Because then the samples array simply gets filled with 0s) (or we can just ignore mp3 haha, this is a bonus feature after all)
            if (!clip || song.audioFile.EndsWith(".mp3"))
            {
                _active = false;
                return;
            }

            _currentSong = song;

            // It's important to multiply by the number of channels!
            var samplesPerBeat = (clip.samples * clip.channels) / (clip.length * song.BeatsPerSec);
            _beatsPerSample = 1f / samplesPerBeat;

            _active = true;
            OpacityTransition(1, .5f);

            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            _samples = samples.Where((s, i) => i % EveryHowManySamples == 0).ToList();
        }

        public override void DrawShapes(Camera cam)
        {
            base.DrawShapes(cam);
            using (Draw.Command(cam))
            {
                if (!_active)
                    return;

                Draw.Matrix = transform.localToWorldMatrix;
                Draw.BlendMode = ShapesBlendMode.Transparent;
                // Draw.ZTest = CompareFunction.Less;
                Draw.LineGeometry = LineGeometry.Flat2D;
                Draw.LineThickness = .02f;
                Draw.Color = new Color(.3f,.3f,.3f);
                Draw.PolylineJoins = PolylineJoins.Round;
                Draw.Opacity = _opacity;

                var xOffset = _currentSong.beatOffset * _currentSong.BeatsPerSec * EditorTrack.viewState.beatSpacing * -1;

                var xPerSample = _beatsPerSample * EditorTrack.viewState.beatSpacing * EveryHowManySamples;
                var samplesPerX = 1f / xPerSample;
                var sampleOffset = Mathf.FloorToInt(Mathf.Max(samplesPerX * (EditorTrack.viewState.panX - xOffset), 0));
                if (sampleOffset >= _samples.Count)
                    return;

                var numberOfSamples = Mathf.FloorToInt(Mathf.Min(samplesPerX * EditorTrack.Instance.containerRect.width, Mathf.Max(_samples.Count - sampleOffset, 0f)));

                using var p = new PolylinePath();
                var points = _samples.GetRange(sampleOffset, numberOfSamples).Select((s, i) => new PolylinePoint(new Vector3(xPerSample*(i+sampleOffset) + xOffset, s * 30f, 20f)));
                p.AddPoints(points);
                Draw.Polyline(p);
            }
        }

        private void OpacityTransition(float op, float dur, float delay = 0) =>
            DOTween.To(() => _opacity, o => _opacity = o, op, dur).SetDelay(delay);

        // todo: find a way to simply show the waveform behind the menu...
        private void OnToggleMenu(bool menuOpened)
        {
            OpacityTransition(menuOpened ? 0 : 1, menuOpened ? .5f : 1f, menuOpened ? 0 : .5f);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DataController.OnToggleMenu += OnToggleMenu;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            DataController.OnToggleMenu -= OnToggleMenu;
        }
    }
}