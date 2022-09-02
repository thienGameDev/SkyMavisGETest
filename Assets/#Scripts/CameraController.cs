using UnityEngine;

namespace _Scripts {
    public class CameraController : MonoBehaviour {
        [SerializeField] private float minSize = 2f;
        [SerializeField] private float maxSize = 10f;

        [SerializeField] private float zoomingSpeed = 10f; 
        // Start is called before the first frame update
        private Camera cam;
        private Vector3 origin;

        private void Start() {
            cam = GetComponent<Camera>();
        }

        // Update is called once per frame
        private void LateUpdate() {
            cam.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * zoomingSpeed;
            if (cam.orthographicSize < minSize) cam.orthographicSize = minSize;
            if (cam.orthographicSize > maxSize) cam.orthographicSize = maxSize;
            PanCamera();
        }

        private void PanCamera() {
            if (Input.GetMouseButtonDown(0)) {
                origin = GetMousePosition();
            }
            
            if (Input.GetMouseButton(0)) {
                Vector3 difference = origin - GetMousePosition();
                transform.position += difference;
            }
            
        }
        private Vector3 GetMousePosition() {
            return cam.ScreenToWorldPoint(Input.mousePosition);
        }
    }
}
