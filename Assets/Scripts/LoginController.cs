using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Database;

public class LoginController : MonoBehaviour
{
    [Header("Inputs (escena IniciarSesion)")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    [Header("Mensajes")]
    public TextMeshProUGUI errorText;

    [Header("Visibilidad de contraseña")]
    public TextMeshProUGUI togglePasswordText;
    public Image togglePasswordImage;
    public Sprite eyeOpenSprite;
    public Sprite eyeClosedSprite;

    [Header("Escenas")]
    public string mainGameSceneName = "PantallaInicial";
    public string recoverSceneName = "RecuperarContrasena";

    private DatabaseReference usersRef;
    private bool isPasswordVisible = false;

    private void Start()
    {
        TryInitUsersRef();
        ClearError();
        ApplyPasswordVisibility();
    }

    private void ClearError()
    {
        if (errorText == null) return;
        errorText.text = "";
        errorText.gameObject.SetActive(false);
    }

    private void ShowError(string message)
    {
        if (errorText == null) return;
        errorText.text = message;
        errorText.gameObject.SetActive(true);
    }

    private void TryInitUsersRef()
    {
        if (usersRef != null) return;

        if (!FirebaseInitializer.IsReady)
        {
            ShowError("Espera un momento, cargando servidor...");
            return;
        }

        try
        {
            usersRef = FirebaseDatabase.DefaultInstance.RootReference.Child("users");
            Debug.Log("LoginController: usersRef inicializado.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("LoginController: No se pudo inicializar usersRef: " + ex);
            ShowError("No se pudo conectar al servidor.");
        }
    }

    public void OnTogglePasswordVisibility()
    {
        isPasswordVisible = !isPasswordVisible;
        ApplyPasswordVisibility();
    }

    private void ApplyPasswordVisibility()
    {
        if (passwordInput == null) return;

        passwordInput.contentType = isPasswordVisible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;

        passwordInput.ForceLabelUpdate();

        if (togglePasswordText != null)
            togglePasswordText.text = isPasswordVisible ? "Ocultar" : "Mostrar";

        if (togglePasswordImage != null)
        {
            if (isPasswordVisible && eyeOpenSprite != null)
                togglePasswordImage.sprite = eyeOpenSprite;
            else if (!isPasswordVisible && eyeClosedSprite != null)
                togglePasswordImage.sprite = eyeClosedSprite;
        }
    }

    public async void OnClickLogin()
    {
        ClearError();

        if (!FirebaseInitializer.IsReady)
        {
            ShowError("Espera un momento, cargando servidor...");
            return;
        }

        if (usersRef == null)
        {
            TryInitUsersRef();
            if (usersRef == null)
            {
                ShowError("Servidor no disponible, intenta de nuevo.");
                return;
            }
        }

        string username = usernameInput != null ? usernameInput.text.Trim() : "";
        string password = passwordInput != null ? passwordInput.text : "";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Falta nombre de usuario o contrasena.");
            return;
        }

        try
        {
            var snapshot = await usersRef.Child(username).GetValueAsync();

            if (!snapshot.Exists)
            {
                ShowError("Nombre de usuario y/o contrasena incorrectos, intenta de nuevo");
                return;
            }

            string json = snapshot.GetRawJsonValue();
            UserData user = JsonUtility.FromJson<UserData>(json);

            string passwordHash = CryptoUtils.Sha256(password);

            if (user.passwordHash != passwordHash)
            {
                ShowError("Nombre de usuario y/o contrasena incorrectos, intenta de nuevo");
                return;
            }

            // 1) Guardar sesion
            if (GameSession.Instance != null)
                GameSession.Instance.SetUser(user);

            // 2) BIND A FIREBASE MANAGERS AHORA MISMO (CLAVE)
            if (FirebaseCoinsManager.Instance != null)
                FirebaseCoinsManager.Instance.BindUser(user.username);

            if (FirebaseInventoryManager.Instance != null)
                FirebaseInventoryManager.Instance.BindUser(user.username);

            Debug.Log("[LOGIN] Bind listo para: " + user.username);

            // 3) Cambiar escena
            SceneManager.LoadScene(mainGameSceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("LoginController: Error al iniciar sesion: " + ex);
            ShowError("Error al iniciar sesion.");
        }
    }

    public void OnClickGoToRecover()
    {
        SceneManager.LoadScene(recoverSceneName);
    }
}
