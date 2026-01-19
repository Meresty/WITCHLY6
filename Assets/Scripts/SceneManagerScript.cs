using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    [Tooltip("Si está activo, imprime logs al cargar escena")]
    [SerializeField] private bool debugLogs = true;

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[SceneManagerScript] sceneName está vacío o null.");
            return;
        }

        // Verifica que la escena exista en Build Settings (sin eso, NO carga)
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[SceneManagerScript] La escena '{sceneName}' NO está en Build Settings o el nombre no coincide.");
            return;
        }

        if (debugLogs)
            Debug.Log($"[SceneManagerScript] Cargando escena: {sceneName}");

        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        if (debugLogs)
            Debug.Log("[SceneManagerScript] QuitGame()");
        Application.Quit();
    }
}
