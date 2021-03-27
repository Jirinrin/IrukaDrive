using Shared;
using TMPro;
using Tools;
using UnityEngine;

namespace BeatmapEditor.SingletonComponents
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ValidationErrorsPanel : MonoBehaviour
    {
        private TextMeshProUGUI _text;

        private void OnValidation(BeatmapValidation.ValidationResult res)
        {
            var txt = "";
            if (!res.IsValid)
            {
                txt = "FAILED TO SAVE CHART";
                if (res.isEmpty)
                    txt += "\n- Chart has no objects";
                else
                {
                    res.overlaps.ForEach(o =>
                        txt += $"\n- Overlap: \"{o.Item1.text}\" (beat {o.Item1.beat}) <=> \"{o.Item2.text}\" (beat {o.Item2.beat})");
                    res.emptyWords.ForEach(w =>
                        txt += $"\n- Empty word at beat {w.beat}");
                    res.invalidChords.ForEach(r =>
                        txt += $"\n- Invalid chord: \"{r.word.text}\" (beat {r.word.beat}){(r.repeated?" [HAS REPEATED CHARS]":"")}{(r.tooLarge?$" [LARGER THAN {C.MaxChordSize}]":"")}");
                }
            }

            _text.text = txt;
        }

        private void OnEnable()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _text.text = "";
            SerializationHelpers.OnBeatmapValidation += OnValidation;
        }
        private void OnDisable()
        {
            SerializationHelpers.OnBeatmapValidation -= OnValidation;
        }
    }
}
