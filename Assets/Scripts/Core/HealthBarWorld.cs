using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Core
{
    public class HealthBarWorld : MonoBehaviour
    {

        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 1, 0);

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

        public void SetHealth(float health)
        {
            if (_slider == null) return;
            float targetValue = GetHealth() - health;
            StopAllCoroutines();
            StartCoroutine(SmoothHealthChange(targetValue));
        }

        IEnumerator SmoothHealthChange(float target)
        {
            while (Mathf.Abs(_slider.value - target) > 0.001f)
            {
                _slider.value = Mathf.Lerp(_slider.value, target, Time.deltaTime * smoothSpeed);
                yield return null;
            }
            _slider.value = target;
        }

        void LateUpdate()
        {
            if (target)
            {
                transform.position = target.position + offset;
                if (Camera.main)
                    transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            }
        }
    }
}
