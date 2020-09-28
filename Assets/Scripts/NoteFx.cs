using TMPro;
using UnityEngine;
using UnityEngine.UI;

// todo: make pulse animation fancy
public class NoteFx : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI characterPrefab = null;
    
    private TextMeshProUGUI _characterObj;
    [SerializeField] private Image circleSpriteNote;
    [SerializeField] private Image circleSpriteTap;

    private const float DefaultCircleScale = 1f;
    private const float MaxCircleScale = 2f;
    
    private float _noteCircleScale = DefaultCircleScale;
    private float _tapCircleScale = DefaultCircleScale;
    
    private void Start()
    {
        // _characterObj = Instantiate(characterPrefab.gameObject, transform).GetComponent<TextMeshProUGUI>();
    }

    private void Pulse()
    {
        _noteCircleScale = MaxCircleScale;
    }
    
    private void OnTap(char _)
    {
        _tapCircleScale = MaxCircleScale;
    }

    private void OnHit(char c, NoteResult result, float? timing)
    {
        Debug.Log("hit");
    }

    private void Update()
    {
        circleSpriteNote.transform.localScale = new Vector3(_noteCircleScale, _noteCircleScale);
        if (_noteCircleScale > DefaultCircleScale)
            _noteCircleScale -= (MaxCircleScale - DefaultCircleScale) / 70;

        circleSpriteTap.transform.localScale = new Vector3(_tapCircleScale, _tapCircleScale);
        if (_tapCircleScale > DefaultCircleScale)
            _tapCircleScale -= (MaxCircleScale - DefaultCircleScale) / 70;
    }
    
    private void OnEnable()
    {
        BeatmapManager.OnNote += Pulse;
        BeatmapManager.OnHit += OnHit;
        PlayerInputManager.OnChar += OnTap;
    }
    private void OnDisable()
    {
        BeatmapManager.OnNote -= Pulse;
        BeatmapManager.OnHit -= OnHit;
        PlayerInputManager.OnChar -= OnTap;
    }
}
