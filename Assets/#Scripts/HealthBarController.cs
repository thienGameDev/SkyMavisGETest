using UnityEngine;

namespace _Scripts {
    public class HealthBarController : MonoBehaviour {
        [SerializeField] private GameObject redHealthBar;
        [SerializeField] private GameObject orangeHealthBar;
        private int _maxHitPoint;
        private Vector3 _localScale;
        private float _vel;
        private float _smoothTime = .25f;
        // Start is called before the first frame update
        void Start() {
            _localScale = orangeHealthBar.transform.localScale;
            var parent = transform.parent.gameObject;
            _maxHitPoint = parent.GetComponent<AxieController>().maxHitPoint;
            var parentId = parent.GetInstanceID();
            var eventUpdateHealthBar = $"UpdateHealthBar{parentId}";
            EventManager.StartListening(eventUpdateHealthBar, UpdateHealthBar);
        }

        private void UpdateHealthBar(int currentHealth) {
            _localScale.x = (float) currentHealth / _maxHitPoint;
            orangeHealthBar.transform.localScale = _localScale;
        }
        // Update is called once per frame
        void Update() {
            var redBarScale = redHealthBar.transform.localScale;
            redBarScale.x = Mathf.SmoothDamp(redBarScale.x, _localScale.x, ref _vel, _smoothTime);
            redHealthBar.transform.localScale = redBarScale;
        }
    }
}
