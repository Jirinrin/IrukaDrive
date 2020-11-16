using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.ScreenControllers.SettingControllers
{
    public class FloatSetting : MonoBehaviour
    {
        [SerializeField] private TMP_Text keyField;
        [SerializeField] private TMP_Text valueField;
        [SerializeField] private Button plusBtn;
        [SerializeField] private Button minusBtn;
        
        private float _modulus;
        private float _value;
        private float _min;
        private float _max;
        private Action<float> _onChange;

        public void Init(string key, float value, float modulus, float min, float max, Action<float> onChange)
        {
            keyField.text = key;
            _value = value;
            _modulus = modulus;
            _min = min;
            _max = max;
            _onChange = onChange;
            UpdateValue();
        }

        private void UpdateValue(bool init = false)
        {
            valueField.text = _value.ToString(CultureInfo.CurrentCulture);
            if (!init)
                _onChange(_value);
        }
        
        private void Start()
        {
            plusBtn.onClick.AddListener(ClickPlus);
            minusBtn.onClick.AddListener(ClickMinus);
        }

        private void ClickPlus()
        {
            _value = Mathf.Min(_value + _modulus, _max);
            UpdateValue();
        }
        private void ClickMinus()
        {
            _value = Mathf.Max(_value - _modulus, _min);
            UpdateValue();
        }
    }
}
