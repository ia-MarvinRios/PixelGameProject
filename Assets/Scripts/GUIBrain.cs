using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GUIBrain : MonoBehaviour
{
    [Header("GUI References")]
    [Space(10)]
    [SerializeField] Slider _healthBar;
    [Space(10)]
    [SerializeField] private string NameScene;
    [SerializeField] private float Wait;
    [SerializeField] private Animator transition;

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

    public void Play()
    {
        if (!string.IsNullOrEmpty(NameScene)) StartCoroutine(LoadLeve());
        else
            Debug.LogError("?? No se ha asignado el nombre de la escena en el Inspector.");
    }

    IEnumerator LoadLeve()
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(Wait);
        SceneManager.LoadScene(NameScene);

    }
}
