using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Core
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 1, 0);
        [SerializeField] private GameObject fillGameObject;

        private Slider _slider;

        private void Start()
        {
            if (_slider == null)
            {
                _slider = GetComponent<Slider>();
            }
        }

        public float GetHealth()
        {
            return _slider?.value ?? 0f;
        }

        public float SetHealth(float health)
        {
            if (_slider == null)
            {
                return 0f;
            }

            var targetValue = Mathf.Max(0, GetHealth() - health);
            StopAllCoroutines();
            StartCoroutine(SmoothHealthChange(targetValue));
            return targetValue;
        }

        private IEnumerator SmoothHealthChange(float targetValue)
        {
            while (Mathf.Abs(_slider.value - targetValue) > 0.001f)
            {
                _slider.value = Mathf.Lerp(_slider.value, targetValue, Time.deltaTime * smoothSpeed);
                yield return null;
            }

            _slider.value = targetValue;
            fillGameObject.SetActive(_slider.value > 0);
        }

        private void LateUpdate()
        {
            if (target)
            {
                transform.position = target.position + offset;
                if (Camera.main)
                {
                    transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
                }
            }
        }
    }
}