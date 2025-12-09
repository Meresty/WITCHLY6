using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Database;

public class PasswordChangeController : MonoBehaviour
{
    [Header("UI - CambioContraseña")]
    public TMP_InputField newPasswordInput;      // primer input
    public TMP_InputField confirmPasswordInput;  // segundo input
    public TextMeshProUGUI errorText;            // texto de error

    [Header("Escenas")]
    public string loginSceneName = "IniciarSesion";

    private DatabaseReference usersRef;

    private void Start()
    {
        TryInitUsersRef();
        ClearError();

        // Si alguien abre esta escena sin pasar por RecuperarCuenta
        if (string.IsNullOrEmpty(PasswordResetData.Username))
        {
            ShowError("Primero recupera tu cuenta desde la pantalla anterior.");
        }
    }

    // ===== Helpers UI =====
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

    // ===== Firebase =====
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
            Debug.Log("PasswordChangeController: usersRef inicializado.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("PasswordChangeController: " + ex);
            ShowError("No se pudo conectar al servidor.");
        }
    }

    // ===== Botón CONTINUAR =====
    public async void OnClickContinue()
    {
        ClearError();

        string pass1 = newPasswordInput.text;
        string pass2 = confirmPasswordInput.text;

        // Validaciones
        if (string.IsNullOrEmpty(pass1) || string.IsNullOrEmpty(pass2))
        {
            ShowError("Contraseña no válida");
            return;
        }

        if (pass1.Length < 5 || pass1.Length > 15)
        {
            ShowError("La contraseña debe tener entre 5 y 15 caracteres.");
            return;
        }

        if (pass1 != pass2)
        {
            ShowError("Contraseña no válida");
            return;
        }

        if (string.IsNullOrEmpty(PasswordResetData.Username))
        {
            ShowError("No hay usuario para actualizar la contraseña.");
            return;
        }

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

        string username = PasswordResetData.Username;

        try
        {
            var snapshot = await usersRef.Child(username).GetValueAsync();
            if (!snapshot.Exists)
            {
                ShowError("El usuario ya no existe.");
                return;
            }

            string json = snapshot.GetRawJsonValue();
            UserData user = JsonUtility.FromJson<UserData>(json);

            // Actualizar password
            string newPassHash = CryptoUtils.Sha256(pass1);
            user.passwordHash = newPassHash;

            string updatedJson = JsonUtility.ToJson(user);
            await usersRef.Child(username).SetRawJsonValueAsync(updatedJson);

            PasswordResetData.Username = null; // limpiamos contexto

            ShowError("Contraseña actualizada correctamente.");
            SceneManager.LoadScene(loginSceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("PasswordChangeController: " + ex);
            ShowError("Error al actualizar la contraseña.");
        }
    }

    public void OnClickBackToLogin()
    {
        SceneManager.LoadScene(loginSceneName);
    }
}
