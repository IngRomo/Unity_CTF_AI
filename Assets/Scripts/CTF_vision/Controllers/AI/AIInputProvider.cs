// AIInputProvider.cs
using UnityEngine;

public class AIInputProvider : MonoBehaviour, IInputProvider {
    [HideInInspector] public Vector2 movementInput;
    [HideInInspector] public bool jumpHeld;
    [HideInInspector] public float rotationInput;

    [HideInInspector] public bool sprintHeld;
    [HideInInspector] public bool dashPressed;
    [HideInInspector] public bool divePressed;

    //* "=> x" is the same as "return x;"
    public Vector2 GetMovement()=> movementInput;
    public bool IsJumpHeld()    => jumpHeld;
    public float GetRotation()  => rotationInput;

    public bool IsSprintHeld()  => sprintHeld;
    public bool IsDashPressed() => dashPressed;
    public bool IsDivePressed() => divePressed;
}
