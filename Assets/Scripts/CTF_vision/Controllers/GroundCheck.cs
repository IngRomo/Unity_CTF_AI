using UnityEngine;

public class GroundCheck : MonoBehaviour {
    private Transform playerTransform;
    public float groundCheckDistance = 1.2f;

    private void Reset() {
        //* Auto-assign playerTransform in editor when component is added or reset
        Rigidbody rb = GetComponentInChildren<Rigidbody>();
        if (rb != null) {
            playerTransform = rb.transform;
        }
    }

    private void OnValidate() {
        //* Auto-assign playerTransform in editor when values change
        if (playerTransform == null) {
            Rigidbody rb = GetComponentInChildren<Rigidbody>();
            if (rb != null) playerTransform = rb.transform;
        }
    }

    public bool IsGrounded() {
        if (playerTransform == null) {
            Debug.LogWarning("GroundCheck: playerTransform not assigned.");
            return false;
        }

        Vector3 origin = playerTransform.position;
        Vector3 direction = Vector3.down;
        return Physics.Raycast(origin, direction, groundCheckDistance);
    }

    private void OnDrawGizmos() {
        if (playerTransform == null) {
            Rigidbody rb = GetComponentInChildren<Rigidbody>();
            if (rb != null) playerTransform = rb.transform;
        }
        if (playerTransform == null) return;

        Gizmos.color = Color.magenta;
        Vector3 origin = playerTransform.position;
        Vector3 direction = Vector3.down;
        Gizmos.DrawRay(origin, direction * groundCheckDistance);
    }
}
