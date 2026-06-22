using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    private Rigidbody rb;
    private Vector2 inputDirection;

    public bool RotationLocked { get; set; } = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical = Input.GetAxisRaw("Vertical");      // W/S

        inputDirection = new Vector2(horizontal, vertical).normalized;
    }

    void FixedUpdate()
    {
        Vector3 moveDirection = new Vector3(inputDirection.x, 0f, inputDirection.y);
        Vector3 targetVelocity = moveDirection * moveSpeed;

        // Preserve vertical velocity (gravity) while controlling horizontal movement
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        // Rotate player to face movement direction (optional, purely visual for now)
        if (!RotationLocked && moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.fixedDeltaTime);
        }
    }
}