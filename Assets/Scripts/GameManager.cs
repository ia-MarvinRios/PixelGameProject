using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
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

    private void GameOver()
    {
        Debug.Log("Player is <color=#FF0000>dead</color>, <color=#ffff>Game Over!</color>");
    }
}
