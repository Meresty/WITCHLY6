using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;

public class RegisterController : MonoBehaviour
{
    [Header("Inputs (Canvas CrearCuenta)")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField questionInput;
    public TMP_InputField answerInput;

    [Header("Mensajes")]
    public TextMeshProUGUI errorText;

    [Header("Escenas")]
    public string loginSceneName = "IniciarSesion";

    [Header("Valores iniciales")]
    public int startingCoins = 10;
    public int startingEnergy = 10;

    private DatabaseReference usersRef;

    private void Start()
    {
        ClearError();
        TryInitUsersRef();
    }

    private void ClearError()
    {
        if (errorText == null) return;
        errorText.text = "";
        errorText.gameObject.SetActive(false);
    }

    private void ShowError(string msg)
    {
        if (errorText == null) return;
        errorText.text = msg;
        errorText.gameObject.SetActive(true);
    }

    private void ShowInfo(string msg)
    {
        if (errorText == null) return;
        errorText.text = msg;
        errorText.gameObject.SetActive(true);
    }

    private void TryInitUsersRef()
    {
        if (usersRef != null) return;

        if (!FirebaseInitializer.IsReady)
        {
            ShowInfo("Espera un momento, cargando servidor...");
            return;
        }

        try
        {
            usersRef = FirebaseDatabase.DefaultInstance.RootReference.Child("users");
            Debug.Log("[REGISTER] usersRef inicializado.");
        }
        catch (Exception ex)
        {
            Debug.LogError("[REGISTER] No se pudo inicializar usersRef: " + ex);
            ShowError("No se pudo conectar al servidor.");
        }
    }

    public async void OnClickRegister()
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
        string question = questionInput != null ? questionInput.text.Trim() : "";
        string answer = answerInput != null ? answerInput.text.Trim() : "";

        // ===== Validaciones segun reglas =====
        if (string.IsNullOrEmpty(username) || username.Length > 15)
        {
            ShowError("Usuario vacio o muy largo (max 15).");
            return;
        }

        // tu regla no valida largo de password en claro, pero tu UX si
        if (string.IsNullOrEmpty(password) || password.Length < 5 || password.Length > 15)
        {
            ShowError("Contrasena de 5 a 15 caracteres.");
            return;
        }

        if (string.IsNullOrEmpty(question) || question.Length > 200)
        {
            ShowError("Pregunta obligatoria (max 200).");
            return;
        }

        if (string.IsNullOrEmpty(answer))
        {
            ShowError("Respuesta obligatoria.");
            return;
        }

        // Hashes (tu regla exige 64 chars)
        string passwordHash = CryptoUtils.Sha256(password);
        string answerHash = CryptoUtils.Sha256(answer);

        if (string.IsNullOrEmpty(passwordHash) || passwordHash.Length != 64)
        {
            ShowError("Error creando hash de contrasena.");
            return;
        }

        if (string.IsNullOrEmpty(answerHash) || answerHash.Length != 64)
        {
            ShowError("Error creando hash de respuesta.");
            return;
        }

        try
        {
            // Verificar si ya existe
            var snapshot = await usersRef.Child(username).GetValueAsync();
            if (snapshot.Exists)
            {
                ShowError("Ese usuario ya existe.");
                return;
            }

            // ===== Armar estructura compatible con reglas ($other = false) =====
            var stats = new Dictionary<string, object>
            {
                { "energy", Mathf.Max(0, startingEnergy) },
                { "progress", 0 },
                { "cardsMade", 0 }
            };

            var inventory = new Dictionary<string, object>
            {
                { "items", new Dictionary<string, object>() }
            };

            var achievements = new Dictionary<string, object>(); // vacio al inicio

            var userData = new Dictionary<string, object>
            {
                { "username", username },
                { "passwordHash", passwordHash },
                { "securityQuestion", question },
                { "securityAnswerHash", answerHash },
                { "coins", Mathf.Max(0, startingCoins) },
                { "stats", stats },
                { "inventory", inventory },
                { "achievements", achievements }
            };

            await usersRef.Child(username).SetValueAsync(userData);

            ShowInfo("Cuenta creada. Redirigiendo...");
            StartCoroutine(GoToLoginAfterDelay(1.2f));
        }
        catch (Exception ex)
        {
            Debug.LogError("[REGISTER] Error al crear cuenta: " + ex);
            ShowError("Error al crear cuenta.");
        }
    }

    private IEnumerator GoToLoginAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(loginSceneName);
    }

    public void OnClickGoToLogin()
    {
        SceneManager.LoadScene(loginSceneName);
    }
}
