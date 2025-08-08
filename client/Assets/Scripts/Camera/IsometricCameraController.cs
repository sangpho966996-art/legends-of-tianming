using UnityEngine;

namespace LegendsOfTianming.Core
{
    public class IsometricCameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        public Transform target;
        public float distance = 10f;
        public float height = 8f;
        public float angle = 35f;
        public float followSpeed = 5f;
        public bool smoothFollow = true;
        
        [Header("Boundaries")]
        public Vector2 minBounds = new Vector2(-50, -50);
        public Vector2 maxBounds = new Vector2(50, 50);
        public bool useBoundaries = true;
        
        [Header("Zoom Settings")]
        public float minZoom = 5f;
        public float maxZoom = 20f;
        public float zoomSpeed = 2f;
        
        private Camera cam;
        private Vector3 offset;
        private float currentZoom;

        private void Start()
        {
            cam = GetComponent<Camera>();
            currentZoom = distance;
            CalculateOffset();
            
            if (target == null)
            {
                var playerManager = GameManager.Instance.GetPlayerManager();
                if (playerManager != null && playerManager.LocalPlayer != null)
                {
                    target = playerManager.LocalPlayer.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;
            
            HandleZoom();
            FollowTarget();
        }

        private void CalculateOffset()
        {
            offset = new Vector3(0, height, -distance);
            offset = Quaternion.Euler(angle, 0, 0) * offset;
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                currentZoom -= scroll * zoomSpeed;
                currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
                
                distance = currentZoom;
                CalculateOffset();
            }
        }

        private void FollowTarget()
        {
            Vector3 targetPosition = target.position + offset;
            
            if (useBoundaries)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.y, maxBounds.y);
            }
            
            if (smoothFollow)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = targetPosition;
            }
            
            Vector3 lookTarget = target.position + Vector3.up * 2f;
            transform.LookAt(lookTarget);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetBoundaries(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
        }

        public void SetZoom(float zoom)
        {
            currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
            distance = currentZoom;
            CalculateOffset();
        }
    }
}
