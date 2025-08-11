// PlayerInputProvider.cs
using UnityEngine;

/// <summary>
/// Wraps the Unity Input System into IInputProvider.
/// Caches WasPressedThisFrame flags and clears them on demand to avoid repeated triggers.
/// </summary>
public class PlayerInputProvider : MonoBehaviour, IInputProvider {
    private PlayerInputActions inputActions;

    private bool dashPressedCached = false;
    private bool divePressedCached = false;

    void Awake() {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
    }

    void OnDisable() {
        inputActions.Player.Disable();
    }

    void Update() {
        dashPressedCached |= inputActions.Player.Dash.WasPressedThisFrame();
        divePressedCached |= inputActions.Player.Dive.WasPressedThisFrame();
    }

    public Vector2 GetMovement() {
        return inputActions.Player.Movement.ReadValue<Vector2>();
    }

    public bool IsJumpHeld() {
        return inputActions.Player.Jump.IsPressed();
    }

    public float GetRotation() {
        return inputActions.Player.Rotate.ReadValue<float>();
    }

    public bool IsSprintHeld() {
        return inputActions.Player.Sprint.IsPressed();
    }

    public bool IsDashPressed() {
        if (dashPressedCached) {
            dashPressedCached = false;
            return true;
        }
        return false;
    }

    public bool IsDivePressed() {
        if (divePressedCached) {
            divePressedCached = false;
            return true;
        }
        return false;
    }
}
