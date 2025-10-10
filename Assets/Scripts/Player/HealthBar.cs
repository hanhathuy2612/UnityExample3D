using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Player
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private float smoothSpeed = 5f;

        private void Start()
        {
            Debug.Log("HealthBar Start 1: " + (slider != null ? slider.ToString() : "null"));
            Debug.Log("Current Health: " + GetHealth());
            Debug.Log("Max Health: " + GetMaxHealth());
        }

        public float GetHealth()
        {
            return slider.value;
        }

        public float GetMaxHealth()
        {
            return slider.maxValue;
        }

        public void SetHealth(float damage)
        {
            float targetValue = GetHealth() - damage;
            StopAllCoroutines();
            StartCoroutine(SmoothHealthChange(targetValue));
        }

        IEnumerator SmoothHealthChange(float target)
        {
            while (Mathf.Abs(slider.value - target) > 0.001f)
            {
                slider.value = Mathf.Lerp(slider.value, target, Time.deltaTime * smoothSpeed);
                yield return null;
            }
            slider.value = target;
        }
    }
}