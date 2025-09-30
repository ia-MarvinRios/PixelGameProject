using UnityEngine;
using UnityEngine.UI;

public class GUIBrain : MonoBehaviour
{
    [Header("GUI References")]
    [Space(10)]
    [SerializeField] Slider _healthBar;

    public delegate void OnHealthBarZeroEvent();
    public static event OnHealthBarZeroEvent onHealthBarZero;
    public static GUIBrain Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    private void OnEnable()
    {
        _healthBar.onValueChanged.AddListener(IsHealthBarZero);
    }
    private void OnDisable()
    {
        _healthBar.onValueChanged.RemoveListener(IsHealthBarZero);
    }

    public float UpdateHealthBarByValue(float value) { return _healthBar.value += value * 0.01f; }
    public void SetHealthBar(float value) { _healthBar.value = value; }
    private void IsHealthBarZero(float value) {
        if (value <= 0)
            onHealthBarZero?.Invoke();
    }
}
