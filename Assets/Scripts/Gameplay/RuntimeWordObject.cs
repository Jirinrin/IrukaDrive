using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class RuntimeWordObject : MonoBehaviour
    {
        public RuntimeWord Word;

        [CanBeNull] public List<TextMeshProUGUI> CharObjRefs;
    }
}