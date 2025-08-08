using UnityEngine;

namespace LegendsOfTianming.Core
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float jumpHeight = 2f;
        public float dashDistance = 8f;
        public float dashCooldown = 3f;
        
        [Header("Camera Settings")]
        public float mouseSensitivity = 2f;
        public float cameraDistance = 10f;
        public float cameraHeight = 8f;
        public float cameraAngle = 35f;

        private CharacterController characterController;
        private Camera playerCamera;
        private bool isLocalPlayer = false;
        private Vector3 targetPosition;
        private bool hasTargetPosition = false;
        private float lastDashTime = 0f;
        private Vector3 velocity;
        private bool isGrounded;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            SetupCamera();
        }

        private void SetupCamera()
        {
            if (!isLocalPlayer) return;

            GameObject cameraObj = new GameObject("PlayerCamera");
            playerCamera = cameraObj.AddComponent<Camera>();
            
            Vector3 offset = new Vector3(0, cameraHeight, -cameraDistance);
            offset = Quaternion.Euler(cameraAngle, 0, 0) * offset;
            
            playerCamera.transform.position = transform.position + offset;
            playerCamera.transform.LookAt(transform.position + Vector3.up * 2f);
            
            playerCamera.fieldOfView = 60f;
            playerCamera.farClipPlane = 1000f;
        }

        private void Update()
        {
            if (isLocalPlayer)
            {
                HandleLocalPlayerInput();
                UpdateCamera();
            }
            else
            {
                HandleRemotePlayerMovement();
            }

            ApplyGravity();
        }

        private void HandleLocalPlayerInput()
        {
            isGrounded = characterController.isGrounded;
            
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
            
            if (direction.magnitude >= 0.1f)
            {
                Vector3 moveDirection = direction * moveSpeed;
                characterController.Move(moveDirection * Time.deltaTime);
                
                transform.rotation = Quaternion.LookRotation(direction);
                
                GameManager.Instance.GetNetworkManager().SendPlayerMove(transform.position, transform.rotation);
            }
            
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            }
            
            if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time - lastDashTime > dashCooldown)
            {
                PerformDash(direction);
            }
        }

        private void HandleRemotePlayerMovement()
        {
            if (hasTargetPosition)
            {
                float distance = Vector3.Distance(transform.position, targetPosition);
                
                if (distance > 0.1f)
                {
                    Vector3 direction = (targetPosition - transform.position).normalized;
                    float moveDistance = moveSpeed * Time.deltaTime;
                    
                    if (moveDistance >= distance)
                    {
                        transform.position = targetPosition;
                        hasTargetPosition = false;
                    }
                    else
                    {
                        characterController.Move(direction * moveDistance);
                        transform.rotation = Quaternion.LookRotation(direction);
                    }
                }
                else
                {
                    hasTargetPosition = false;
                }
            }
        }

        private void PerformDash(Vector3 direction)
        {
            if (direction.magnitude < 0.1f)
                direction = transform.forward;
            
            Vector3 dashVector = direction.normalized * dashDistance;
            characterController.Move(dashVector);
            
            lastDashTime = Time.time;
            
            Debug.Log("💨 Dash performed");
        }

        private void ApplyGravity()
        {
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            
            velocity.y += Physics.gravity.y * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        private void UpdateCamera()
        {
            if (playerCamera == null || !isLocalPlayer) return;
            
            Vector3 offset = new Vector3(0, cameraHeight, -cameraDistance);
            offset = Quaternion.Euler(cameraAngle, 0, 0) * offset;
            
            Vector3 targetPosition = transform.position + offset;
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, targetPosition, Time.deltaTime * 5f);
            
            Vector3 lookTarget = transform.position + Vector3.up * 2f;
            playerCamera.transform.LookAt(lookTarget);
        }

        public void SetAsLocalPlayer(bool isLocal)
        {
            isLocalPlayer = isLocal;
            
            if (isLocal)
            {
                SetupCamera();
            }
            else if (playerCamera != null)
            {
                Destroy(playerCamera.gameObject);
            }
        }

        public void SetTargetPosition(Vector3 position)
        {
            targetPosition = position;
            hasTargetPosition = true;
        }

        public bool IsLocalPlayer => isLocalPlayer;
        public Vector3 GetVelocity() => velocity;
        public bool IsGrounded => isGrounded;
    }
}
