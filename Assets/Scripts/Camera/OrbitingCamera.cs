/// OrbitingCamera.cs
using UnityEngine;

public class OrbitingCamera : MonoBehaviour {
    [SerializeField] private Rigidbody targetRb = null;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -15f);
    [SerializeField] private float orbitSpeed = 75f; // degrees per second

    private float currentAngle = 0f;

    void LateUpdate() {
        if (targetRb == null)
            return;

        // Orbit around the target on Y axis
        currentAngle += orbitSpeed * Time.deltaTime;
        currentAngle %= 360f;

        // Calculate position
        Quaternion rotation = Quaternion.Euler(0f, currentAngle, 0f);
        Vector3 desiredPos = targetRb.position + rotation * offset;

        transform.position = desiredPos;

        // Always look at the target's position
        transform.LookAt(targetRb.position);
    }
}
