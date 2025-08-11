/// StaminaComponent.cs
using UnityEngine;
using System;

public class StaminaComponent : MonoBehaviour
{
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenPerSecond = 15f;
    [Tooltip("Stamina must regenerate to this amount after being exhausted before it can be used again.")]
    [SerializeField] private float staminaRegenThreshold = 20f;

    private float stamina;
    private bool isExhausted = false;

    public float Stamina => stamina;
    public float MaxStamina => maxStamina;
    public bool IsExhausted => isExhausted;

    // Event: (current, max)
    public event Action<float, float> OnStaminaChanged;

    private void Awake() {
        stamina = maxStamina;
    }

    private void Update() {
        // If we are exhausted, we cannot use stamina. We must wait for it to regen
        // past the threshold.
        if (isExhausted) {
            if (stamina >= staminaRegenThreshold) {
                isExhausted = false;
                Debug.Log("[Stamina] Recovered from exhaustion.");
            }
        }

        // Passive regen on Update so UI updates smoothly each frame.
        float old = stamina;
        stamina = Mathf.Min(maxStamina, stamina + regenPerSecond * Time.deltaTime);

        if (!Mathf.Approximately(old, stamina)) {
            OnStaminaChanged?.Invoke(stamina, maxStamina);
        }
    }

    // Try to subtract; returns true if enough stamina and not exhausted.
    public bool TryConsume(float amount) {
        if (isExhausted || stamina < amount) {
            if(isExhausted) Debug.LogWarning("[Stamina] Action failed: Exhausted.");
            else Debug.LogWarning($"[Stamina] Not enough stamina! Needed: {amount}, have: {stamina}/{maxStamina}");
            return false;
        }
        
        stamina -= amount;
        OnStaminaChanged?.Invoke(stamina, maxStamina);

        // Check if we just became exhausted
        if (stamina <= 0) {
            isExhausted = true;
            stamina = 0; // Clamp to zero
            Debug.LogWarning("[Stamina] Stamina exhausted!");
        }
        return true;
    }

    // Forcefully subtract (used per-frame sprint drain)
    public void ForceConsume(float amount) {
        if (isExhausted) return; // Cannot force consume if exhausted

        float oldStamina = stamina;
        stamina = Mathf.Max(0f, stamina - amount); // Consume stamina first

        if (!Mathf.Approximately(oldStamina, stamina)) {
            OnStaminaChanged?.Invoke(stamina, maxStamina);
        }

        // Check if we just became exhausted
        if (stamina <= 0) {
            isExhausted = true;
            Debug.LogWarning("[Stamina] Stamina exhausted!");
        }
    }

    public void Restore(float amount) {
        float old = stamina;
        stamina = Mathf.Min(maxStamina, stamina + amount);
        if (!Mathf.Approximately(old, stamina))
        {
            Debug.Log($"[Stamina] Restored {amount}, current: {stamina}/{maxStamina}");
            OnStaminaChanged?.Invoke(stamina, maxStamina);
        }
    }

    // Utility: get normalized value 0..1
    public float Normalized => (maxStamina > 0f) ? (stamina / maxStamina) : 0f;
}
