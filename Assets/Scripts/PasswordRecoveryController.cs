using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Database;

public class PasswordRecoveryController : MonoBehaviour
{
    [Header("UI - RecuperarCuenta")]
    public TMP_InputField usernameInput;     // IF_Username
    public TextMeshProUGUI questionText;     // Txt_PreguntaDeUsuario
    public TMP_InputField answerInput;       // IF_Respuesta
    public TextMeshProUGUI errorText;        // Txt_ErrorRecuperar (lo creamos ahorita)

    [Header("Escenas")]
    public string changePasswordSceneName = "CambioContrasena"; // <- pon EXACTO el nombre de tu escena CambioContraseña
    public string loginSceneName = "IniciarSesion";             // escena de login

    private DatabaseReference usersRef;
    private bool userValidated = false;  // false = estamos validando usuario, true = validando respuesta
    private string currentUsername;
    private string storedAnswerHash;

    private void Start()
    {
        TryInitUsersRef();
        ClearError();
        SetQuestionAndAnswerVisible(false);  // al inicio solo se usa el campo de usuario
    }

    // ========= Helpers UI =========
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

    private void SetQuestionAndAnswerVisible(bool visible)
    {
        if (questionText != null)
            questionText.gameObject.SetActive(visible);

        if (answerInput != null)
            answerInput.gameObject.SetActive(visible);
    }

    // ========= Firebase =========
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
            Debug.Log("PasswordRecoveryController: usersRef inicializado.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("PasswordRecoveryController: " + ex);
            ShowError("No se pudo conectar al servidor.");
        }
    }

    // ========= Botón VERIFICAR (dos fases) =========
    public async void OnClickVerify()
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

        // FASE 1: validar usuario
        if (!userValidated)
        {
            string username = usernameInput.text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                ShowError("Ingresa tu nombre de usuario.");
                return;
            }

            try
            {
                var snapshot = await usersRef.Child(username).GetValueAsync();

                if (!snapshot.Exists)
                {
                    // Usuario no existe
                    ShowError("Nombre de usuario incorrecto");
                    SetQuestionAndAnswerVisible(false);
                    currentUsername = null;
                    storedAnswerHash = null;
                    return;
                }

                // Usuario correcto ? mostrar pregunta
                string json = snapshot.GetRawJsonValue();
                UserData user = JsonUtility.FromJson<UserData>(json);

                currentUsername = user.username;
                storedAnswerHash = user.securityAnswerHash;

                if (questionText != null)
                    questionText.text = user.securityQuestion;

                answerInput.text = "";
                SetQuestionAndAnswerVisible(true);
                userValidated = true;

                ShowError("Ahora responde la pregunta de seguridad.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("PasswordRecoveryController (fase usuario): " + ex);
                ShowError("Error al buscar el usuario.");
            }
        }
        // FASE 2: validar respuesta y pasar a CambioContraseña
        else
        {
            string answer = answerInput.text.Trim();

            if (string.IsNullOrEmpty(answer))
            {
                ShowError("Escribe la respuesta de seguridad.");
                return;
            }

            try
            {
                string answerHash = CryptoUtils.Sha256(answer);

                if (!string.IsNullOrEmpty(storedAnswerHash) &&
                    storedAnswerHash != answerHash)
                {
                    ShowError("Respuesta incorrecta a la pregunta de seguridad.");
                    return;
                }

                // OK ? guardamos el usuario para la siguiente escena
                PasswordResetData.Username = currentUsername;

                // Cambiar a escena de CambioContraseña
                SceneManager.LoadScene(changePasswordSceneName);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("PasswordRecoveryController (fase respuesta): " + ex);
                ShowError("Error al validar la respuesta.");
            }
        }
    }

    public void OnClickBackToLogin()
    {
        SceneManager.LoadScene(loginSceneName);
    }
}
