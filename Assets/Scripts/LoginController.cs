using UnityEngine;
using UnityEngine.UI;       // ?? IMPORTANTE para Image y Sprite
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Database;

public class LoginController : MonoBehaviour
{
    [Header("Inputs (escena IniciarSesion)")]
    public TMP_InputField usernameInput;       // IF_Username
    public TMP_InputField passwordInput;       // IF_Contraseña

    [Header("Mensajes")]
    public TextMeshProUGUI errorText;          // Text_ErrorLogin

    [Header("Visibilidad de contraseña")]
    public TextMeshProUGUI togglePasswordText; // (opcional) Texto "Mostrar/Ocultar"
    public Image togglePasswordImage;          // Imagen del ojito
    public Sprite eyeOpenSprite;               // Sprite ojo ABIERTO
    public Sprite eyeClosedSprite;             // Sprite ojo CERRADO

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

    // =====================
    // Helpers para errores
    // =====================
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

    // =====================
    // Inicializar Firebase
    // =====================
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
            usersRef = FirebaseDatabase.DefaultInstance
                                       .RootReference
                                       .Child("users");
            Debug.Log("LoginController: usersRef inicializado.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("LoginController: No se pudo inicializar usersRef: " + ex);
            ShowError("No se pudo conectar al servidor.");
        }
    }

    // =====================
    // Toggle de contraseña
    // =====================
    public void OnTogglePasswordVisibility()
    {
        isPasswordVisible = !isPasswordVisible;
        ApplyPasswordVisibility();
    }

    private void ApplyPasswordVisibility()
    {
        if (passwordInput == null) return;

        // Mostrar u ocultar caracteres
        passwordInput.contentType = isPasswordVisible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;

        passwordInput.ForceLabelUpdate();

        // (Opcional) cambiar texto "Mostrar"/"Ocultar"
        if (togglePasswordText != null)
            togglePasswordText.text = isPasswordVisible ? "Ocultar" : "Mostrar";

        // Cambiar sprite del ojito
        if (togglePasswordImage != null)
        {
            if (isPasswordVisible && eyeOpenSprite != null)
                togglePasswordImage.sprite = eyeOpenSprite;
            else if (!isPasswordVisible && eyeClosedSprite != null)
                togglePasswordImage.sprite = eyeClosedSprite;
        }
    }

    // =====================
    // Login
    // =====================
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

        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        // Campos vacíos
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Falta nombre de usuario o contraseña.");
            return;
        }

        try
        {
            var snapshot = await usersRef.Child(username).GetValueAsync();

            // Usuario no encontrado
            if (!snapshot.Exists)
            {
                ShowError("Nombre de usuario y/o contraseña incorrectos, Intenta de nuevo");
                return;
            }

            string json = snapshot.GetRawJsonValue();
            UserData user = JsonUtility.FromJson<UserData>(json);

            string passwordHash = CryptoUtils.Sha256(password);

            // Contraseña incorrecta
            if (user.passwordHash != passwordHash)
            {
                ShowError("Nombre de usuario y/o contraseña incorrectos, Intenta de nuevo");
                return;
            }

            // Login correcto
            if (GameSession.Instance != null)
                GameSession.Instance.SetUser(user);

            SceneManager.LoadScene(mainGameSceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("LoginController: Error al iniciar sesión: " + ex);
            ShowError("Error al iniciar sesión.");
        }
    }

    // =====================
    // Ir a recuperar contraseña
    // =====================
    public void OnClickGoToRecover()
    {
        SceneManager.LoadScene(recoverSceneName);
    }
}
