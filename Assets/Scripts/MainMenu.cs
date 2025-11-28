using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] AudioSource _menuAudioSource;
    private void Start()
    {
        if(_menuAudioSource != null) _menuAudioSource.Play();
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void LoadScene(string name = "Menu")
    {
        StartCoroutine(LoadSceneAndWait(name));
    }
    public IEnumerator LoadSceneAndWait(string sceneName)
    {
        // Comienza la carga asíncrona
        AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // Esperar a que la escena llegue al 90%
        // (Unity carga hasta 0.9 y luego espera activación)
        while (op.progress < 0.9f)
        {
            // Aquí puedes actualizar barras de carga
            yield return null;
        }

        Debug.Log("Scene loaded but waiting for systems...");

        // Activar la escena
        op.allowSceneActivation = true;

        Debug.Log("Scene fully initialized and activated!");
    }
}
