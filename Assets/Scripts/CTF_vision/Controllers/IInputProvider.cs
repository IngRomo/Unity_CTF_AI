/// IInputProvider.cs
using UnityEngine;

/// <summary>
/// Provides an abstraction for input sources (Player, AI, replay, etc.).
/// </summary>
public interface IInputProvider {
    /// <summary>Get movement vector (X,Z).</summary>
    Vector2 GetMovement();

    /// <summary>Get current rotation.</summary>
    float GetRotation();

    /// <summary>Jump  key held.</summary>
    bool IsJumpHeld();

    /// <summary>Sprint key held.</summary>
    bool IsSprintHeld();

    /// <summary>Dash key held.</summary>
    bool IsDashPressed();

    /// <summary>Dive key held.</summary>
    bool IsDivePressed();
}