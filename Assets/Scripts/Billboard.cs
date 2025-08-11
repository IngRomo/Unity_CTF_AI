using UnityEngine;

public class Billboard : MonoBehaviour {
    private Camera camera;
    void Start() {
        camera = Camera.main;
    }

    private void LateUpdate() {
        transform.forward = camera.transform.forward;
    }
}
