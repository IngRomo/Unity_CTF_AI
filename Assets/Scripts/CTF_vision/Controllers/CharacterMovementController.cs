// CharacterMovementController.cs
using UnityEngine;

/// <summary>
/// Movement controller: walk, sprint, jump, dash, dive with stamina and cooldowns.
/// Dash triggers only if executable immediately when pressed (no input buffering).
/// </summary>
[RequireComponent(typeof(GroundCheck))]
[RequireComponent(typeof(StaminaComponent))]
public class CharacterMovementController : MonoBehaviour {
    [Tooltip("Assign a component implementing IInputProvider (PlayerInputProvider or AIInputProvider).")]
    [SerializeField] private MonoBehaviour inputProviderComponent = null;
    private IInputProvider inputProvider;

    [Header("Hierarchy")]
    [SerializeField] private Rigidbody rigidBody = null;

    [SerializeField] private StaminaComponent staminaComponent = null;
    private GroundCheck groundCheck;

    [Header("Walking")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float smoothTime = 0.08f;
    private Vector3 velocitySmoothDamp;

    [Header("Sprinting")]
    [SerializeField] private float sprintMultiplier = 2.0f;
    [SerializeField] private float sprintDrainPerSecond = 40f;

    [Header("Jump")]
    [SerializeField] private float jumpStrength = 4.5f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float doubleJumpCost = 8f;
    private int jumpsRemaining;
    private bool jumpedThisGround = false;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 1.0f;
    [SerializeField] private float dashCost = 30f;
    [SerializeField] private bool disableControlDuringDash = true;
    private float dashTimer = 0f;
    private float dashCdTimer = 0f;

    [Header("Dive")]
    [SerializeField] private float diveSpeed = 14f;
    [SerializeField] private float diveDownMultiplier = 1.2f;
    [SerializeField] private float diveCost = 10f;

    [Header("Gravity / Rotation")]
    [SerializeField] private float fallMultiplier = 3f;
    [SerializeField] private float rotationSpeed = 220f;

    private bool dashRequest = false;

    void Awake() {
        inputProvider = inputProviderComponent as IInputProvider;
        if (inputProvider == null) {
            Debug.LogError("InputProviderComponent must implement IInputProvider.");
            enabled = false;
            return;
        }
        if (rigidBody == null) {
            rigidBody = GetComponentInChildren<Rigidbody>();
            if (rigidBody == null) {
                Debug.LogError("Assign a Rigidbody (self or child).");
                enabled = false;
                return;
            }
        }
        if (staminaComponent == null) staminaComponent = GetComponent<StaminaComponent>();
        groundCheck = GetComponent<GroundCheck>();
        jumpsRemaining = maxJumps;
    }

    void Update() {
        if (inputProvider.IsDashPressed() && CanDashNow())
            dashRequest = true;
    }

    bool CanDashNow() {
        if (dashCdTimer > 0f || dashTimer > 0f) return false;
        if (staminaComponent.IsExhausted) return false;
        return staminaComponent.Stamina >= dashCost;
    }

    void FixedUpdate() {
        if (dashCdTimer > 0f) dashCdTimer -= Time.fixedDeltaTime;
        if (dashTimer > 0f) dashTimer -= Time.fixedDeltaTime;

        bool isGrounded = groundCheck.IsGrounded();
        if (isGrounded) {
            jumpsRemaining = maxJumps;
            jumpedThisGround = false;
        }

        Vector2 move2D = inputProvider.GetMovement();
        bool sprintHeld = inputProvider.IsSprintHeld();
        bool jumpHeld = inputProvider.IsJumpHeld();
        bool divePressed = inputProvider.IsDivePressed();
        float rotInput = inputProvider.GetRotation();

        if (dashRequest) {
            if (dashCdTimer <= 0f && dashTimer <= 0f && staminaComponent.TryConsume(dashCost)) {
                Vector3 dashLocal = new Vector3(move2D.x, 0f, move2D.y);
                Vector3 dashWorld = dashLocal.sqrMagnitude < 0.01f
                    ? rigidBody.transform.forward
                    : rigidBody.transform.TransformDirection(dashLocal.normalized);

                rigidBody.velocity = new Vector3(dashWorld.x * dashSpeed, rigidBody.velocity.y, dashWorld.z * dashSpeed);
                dashTimer = dashDuration;
                dashCdTimer = dashCooldown;
            }
            dashRequest = false;
        }

        if (divePressed && dashCdTimer <= 0f && dashTimer <= 0f && staminaComponent.TryConsume(diveCost)) {
            Vector3 diveLocal = new Vector3(move2D.x, -diveDownMultiplier, move2D.y);
            Vector3 diveWorld = rigidBody.transform.TransformDirection(diveLocal.normalized);
            rigidBody.velocity = new Vector3(diveWorld.x * diveSpeed, diveWorld.y * diveSpeed, diveWorld.z * diveSpeed);
            dashTimer = dashDuration;
            dashCdTimer = dashCooldown;
        }

        if (dashTimer > 0f && disableControlDuringDash) {
            ApplyFastFall();
            if (Mathf.Abs(rotInput) > 0.01f) {
                rigidBody.MoveRotation(rigidBody.rotation * Quaternion.Euler(0f, rotInput * rotationSpeed * Time.fixedDeltaTime, 0f));
            }
            return;
        }

        bool sprinting = false;
        float speed = walkSpeed;
        bool moving = move2D.sqrMagnitude > 0.01f;
        if (sprintHeld && moving && !staminaComponent.IsExhausted && staminaComponent.Stamina > 0f) {
            sprinting = true;
            staminaComponent.ForceConsume(sprintDrainPerSecond * Time.fixedDeltaTime);
        }
        if (sprinting) speed *= sprintMultiplier;

        Vector3 desiredWorld = rigidBody.transform.TransformDirection(new Vector3(move2D.x, 0f, move2D.y)) * speed;
        Vector3 currentXZ = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
        Vector3 targetXZ = new Vector3(desiredWorld.x, 0f, desiredWorld.z);
        Vector3 smoothXZ = Vector3.SmoothDamp(currentXZ, targetXZ, ref velocitySmoothDamp, smoothTime, Mathf.Infinity, Time.fixedDeltaTime);
        rigidBody.velocity = new Vector3(smoothXZ.x, rigidBody.velocity.y, smoothXZ.z);

        if (jumpHeld && !jumpedThisGround && jumpsRemaining > 0) {
            bool canJump = true;
            if (jumpsRemaining < maxJumps && doubleJumpCost > 0f) {
                canJump = staminaComponent.TryConsume(doubleJumpCost);
            }
            if (canJump) {
                Vector3 v = rigidBody.velocity; v.y = 0f; rigidBody.velocity = v;
                rigidBody.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
                jumpsRemaining--;
                jumpedThisGround = true;
            }
        }
        if (!jumpHeld) jumpedThisGround = false;

        ApplyFastFall();

        if (Mathf.Abs(rotInput) > 0.01f) {
            rigidBody.MoveRotation(rigidBody.rotation * Quaternion.Euler(0f, rotInput * rotationSpeed * Time.fixedDeltaTime, 0f));
        }
    }

    void ApplyFastFall() {
        if (rigidBody.velocity.y < 0f) {
            rigidBody.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }
}
