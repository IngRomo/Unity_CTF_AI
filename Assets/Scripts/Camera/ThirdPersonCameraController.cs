// ThirdPersonCameraController.cs
using UnityEngine;

[DefaultExecutionOrder(100)]
public class ThirdPersonCameraController : MonoBehaviour {
    [Header("Target")]
    [SerializeField] private Transform target = null;               // player transform
    private Rigidbody targetRb = null;                              // cached Rigidbody for interpolation
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.7f, 0f);

    [Header("Orbit")]
    [SerializeField] private float yaw = 0f;
    [SerializeField] private float pitch = 20f;
    [SerializeField] private Vector2 pitchLimits = new Vector2(-30f, 75f);
    [SerializeField] private Vector2 lookSensitivity = new Vector2(180f, 120f);

    [Header("Distance / Zoom")]
    [SerializeField] private float distance = 4.0f;
    [SerializeField] private float minDistance = 1.0f;
    [SerializeField] private float maxDistance = 7.0f;
    [SerializeField] private float zoomSpeed = 5f;

    [Header("Smoothing")]
    [SerializeField] private float rotationSmoothTime = 0.05f;
    [SerializeField] private float positionSmoothTime = 0.04f;

    [Header("Collision")]
    [SerializeField] private float cameraRadius = 0.2f;
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float collisionBuffer = 0.05f;

    [Header("Input")]
    [SerializeField] private bool useExternalInput = false;
    [SerializeField] private bool lockCursor = true;

    private Vector2 externalLook;
    private float externalZoom;

    private Vector3 posVelocity;
    private Vector2 rotVelocity;
    private Vector2 smoothedAngles;

    void Start() {
        if (target == null) {
            Debug.LogWarning("[TPCam] No target assigned.");
            enabled = false;
            return;
        }

        targetRb = target.GetComponent<Rigidbody>();
        if (targetRb == null) {
            Debug.LogWarning("[TPCam] Target Rigidbody not found. Consider adding Rigidbody interpolation for smooth camera.");
        }

        smoothedAngles = new Vector2(yaw, pitch);

        if (lockCursor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate() {
        if (target == null)
            return;

        Vector2 look;
        float zoomDelta = 0f;

        if (useExternalInput) {
            look = externalLook;
            zoomDelta = externalZoom;
            externalLook = Vector2.zero;
            externalZoom = 0f;
        } else {
            look = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            zoomDelta = Input.GetAxisRaw("Mouse ScrollWheel");
        }

        yaw += look.x * lookSensitivity.x * Time.unscaledDeltaTime;
        pitch -= look.y * lookSensitivity.y * Time.unscaledDeltaTime;
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);

        if (Mathf.Abs(zoomDelta) > 0.0001f) {
            distance = Mathf.Clamp(distance - zoomDelta * zoomSpeed, minDistance, maxDistance);
        }

        smoothedAngles.x = Mathf.SmoothDampAngle(smoothedAngles.x, yaw, ref rotVelocity.x, rotationSmoothTime);
        smoothedAngles.y = Mathf.SmoothDampAngle(smoothedAngles.y, pitch, ref rotVelocity.y, rotationSmoothTime);
        Quaternion camRot = Quaternion.Euler(smoothedAngles.y, smoothedAngles.x, 0f);

        Vector3 pivot = targetRb != null ? targetRb.position + targetOffset : target.position + targetOffset;

        Vector3 desiredPos = pivot - (camRot * Vector3.forward) * distance;

        Vector3 toCam = desiredPos - pivot;
        float rayLen = toCam.magnitude;
        Vector3 safePos = desiredPos;

        if (rayLen > 0.001f) {
            if (Physics.SphereCast(pivot, cameraRadius, toCam.normalized, out RaycastHit hit, rayLen, collisionMask, QueryTriggerInteraction.Ignore)) {
                safePos = hit.point + hit.normal * (cameraRadius + collisionBuffer);
            }
        }

        transform.position = Vector3.SmoothDamp(transform.position, safePos, ref posVelocity, positionSmoothTime);
        transform.rotation = camRot;
    }

    public void SetLookInput(Vector2 lookDelta) {
        externalLook = lookDelta;
    }

    public void SetZoomInput(float zoomDelta) {
        externalZoom = zoomDelta;
    }

    public void SnapBehindTarget() {
        Vector3 forward = target.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 0.0001f) {
            yaw = Quaternion.LookRotation(forward, Vector3.up).eulerAngles.y;
            smoothedAngles.x = yaw;
        }
    }

    public void SetTarget(Transform t) {
        target = t;
        targetRb = t.GetComponent<Rigidbody>();
    }
}
