using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private float _timeOnLevel = 0f;
    bool gameOver = false;

    public static GameManager Instance { get; private set; }
    public float TimeOnLevel { get { return _timeOnLevel; } }
    public bool GameIsOver { get { return gameOver; } }

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "Menu")
            AudioManager.Instance.PlaySoundByName("Reloaded");
            ConfineCursor();
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

    public void LoadScene(string name = "Menu")
    {
        SceneManager.LoadScene(name, LoadSceneMode.Single);
    }
    void GameOver()
    {
        if (!gameOver)
        {
            gameOver = true;
            Debug.Log("Player is <color=#FF0000>dead</color>, <color=#ffff>Game Over!</color>");
            GUIBrain.Instance._gameOverScreen.SetActive(true);
            Animator animator = GUIBrain.Instance._gameOverScreen.GetComponent<Animator>();
            animator.SetTrigger("Start");

            UnlockCursor();
        }
    }
    public void ConfineCursor()
    {
        // Hide and lock the cursor.
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }
    public void UnlockCursor()
    {
        // Show and unlock the cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
