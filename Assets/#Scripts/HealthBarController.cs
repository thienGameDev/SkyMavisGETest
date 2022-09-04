using System;
using UnityEngine;

namespace _Scripts {
    public class HealthBarController : MonoBehaviour {
        [SerializeField] private GameObject innerHealthBar;
        [SerializeField] private GameObject outerHealthBar;
        private string _eventUpdateHealthBar;
        private Vector3 _localScale;
        private int _maxHitPoint;
        private bool _quit;
        private float _smoothTime = .25f;
        private float _vel;

        // Start is called before the first frame update
        void Start() {
            _localScale = outerHealthBar.transform.localScale;
            var parent = transform.parent.gameObject;
            _maxHitPoint = parent.GetComponent<AxieController>().maxHitPoint;
            var parentId = parent.GetInstanceID();
            _eventUpdateHealthBar = "UpdateHealthBar" + parentId;
            EventManager.StartListening(_eventUpdateHealthBar, UpdateHealthBar);
        }

        // Update is called once per frame
        private void Update() {
            var localScale = innerHealthBar.transform.localScale;
            localScale.x = Mathf.SmoothDamp(localScale.x, _localScale.x, ref _vel, _smoothTime);
            innerHealthBar.transform.localScale = localScale;
        }

        private void OnDisable() {
            if (_quit) return;
            EventManager.StopListening(_eventUpdateHealthBar, UpdateHealthBar);
        }

        private void OnApplicationQuit() {
            _quit = true;
        }

        private void UpdateHealthBar(int currentHealth) {
            _localScale.x = (float) currentHealth / _maxHitPoint;
            outerHealthBar.transform.localScale = _localScale;
        }
    }
}
