using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInputBridge : MonoBehaviour {
    public ThirdPersonCameraController cam;
    private PlayerInputActions actions;
    void Awake() {
        actions = new PlayerInputActions();
        actions.Enable();
    }
    void Update() {
        cam.SetLookInput(actions.Camera.Look.ReadValue<Vector2>());
        cam.SetZoomInput(actions.Camera.Zoom.ReadValue<float>());
    }
    void OnDestroy() { actions.Disable(); }
}
