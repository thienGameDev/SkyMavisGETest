using UnityEngine;

namespace _Scripts {
    public class HealthBarController : MonoBehaviour {
        [SerializeField] private GameObject innerHealthBar;
        [SerializeField] private GameObject outerHealthBar;
        private int _currentHitPoint;
        private string _eventUpdateHealthBar;
        private int _maxHitPoint;
        private bool _quit;
        private float _smoothTime = .25f;
        private float _vel;

        // Start is called before the first frame update
        void Start() {
            var parent = transform.parent.gameObject;
            var axieController = parent.GetComponent<AxieController>();
            _maxHitPoint = axieController.maxHitPoint;
            _currentHitPoint =_maxHitPoint;
        }

        // Update is called once per frame
        private void Update() {
            var localScale = innerHealthBar.transform.localScale;
            var targetScale = GetScaleXAmount();
            localScale.x = Mathf.SmoothDamp(localScale.x, targetScale, ref _vel, _smoothTime);
            innerHealthBar.transform.localScale = localScale;
        }

        private void OnDisable() {
            if (_quit) return;
            EventManager.StopListening(_eventUpdateHealthBar, UpdateHealthBar);
        }

        private void OnApplicationQuit() {
            _quit = true;
        }

        private float GetScaleXAmount() {
            return (float) _currentHitPoint / _maxHitPoint;
            
        }

        public void UpdateHealthBar(int currentHealth) {
            _currentHitPoint = currentHealth;
            var localScale = outerHealthBar.transform.localScale;
            localScale.x = GetScaleXAmount();
            outerHealthBar.transform.localScale = localScale;
        }
    }
}
