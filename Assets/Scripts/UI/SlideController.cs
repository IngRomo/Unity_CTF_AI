using UnityEngine;
using UnityEngine.UI;

public class SlideController : MonoBehaviour {
    [SerializeField] private Slider slider;
    [SerializeField] private StaminaComponent staminaComponent;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image fill;

    private float targetValue;

    private void Awake() {
        slider.minValue = 0f;
        slider.maxValue = staminaComponent.MaxStamina;
        
        targetValue = slider.maxValue;
        slider.value = slider.maxValue;

        staminaComponent.OnStaminaChanged += (current, max) => targetValue = current;
    }

    private void Update() {
        slider.value = Mathf.Lerp(slider.value, targetValue, Time.deltaTime * 10f);
        fill.color = gradient.Evaluate(slider.value / slider.maxValue);
    }
}

