using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private float _timeOnLevel = 0f;
    bool gameOver = false;
    int _kills = 0;

    [Header("Level Settings")]
    [Tooltip("Cantidad de tiempo que dura el nivel en minutos")]
    [SerializeField] float _levelTargetTime = 3;
    [SerializeField] string _levelSong = "Reloaded";

    public static GameManager Instance { get; private set; }
    public float TimeOnLevel { get { return _timeOnLevel; } }
    public bool GameIsOver { get { return gameOver; } }
    public float LevelTargetTime {  get { return _levelTargetTime; } }
    public int Kills { get { return _kills; } }

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        AudioManager.Instance.PlaySoundByName(_levelSong, null);
        if (SceneManager.GetActiveScene().name != "Menu")
            ConfineCursor();
    }
    private void OnEnable()
    {
        GUIBrain.onHealthBarZero += GameOver;
        EnemyController.onEnemyDie += UpdateKills;
    }
    private void OnDisable()
    {
        GUIBrain.onHealthBarZero -= GameOver;
    }
    private void Update()
    {
        _timeOnLevel += Time.deltaTime;
    }

    GameObject UpdateKills(GameObject enemy)
    {
        _kills++;
        GUIBrain.Instance.UpdateKillsUI(_kills);
        return null;
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
    public void LoadScene(string name = "Menu")
    {
        SceneManager.LoadScene(name, LoadSceneMode.Single);
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
