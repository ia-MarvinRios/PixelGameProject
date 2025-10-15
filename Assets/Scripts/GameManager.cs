using UnityEngine;

public class GameManager : MonoBehaviour
{
    private float _timeOnLevel = 0f;

    public static GameManager Instance { get; private set; }
    public float TimeOnLevel { get { return _timeOnLevel; } }
    

    private void Awake()
    {
        Instance = this;
    }
    private void OnEnable()
    {
        GUIBrain.onHealthBarZero += GameOver;
    }
    private void OnDisable()
    {
        GUIBrain.onHealthBarZero -= GameOver;
    }
    private void Update()
    {
        _timeOnLevel += Time.deltaTime;
    }

    private void GameOver()
    {
        Debug.Log("Player is <color=#FF0000>dead</color>, <color=#ffff>Game Over!</color>");
    }
}
