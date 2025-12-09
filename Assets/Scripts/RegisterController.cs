using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Database;

public class RegisterController : MonoBehaviour
{
    [Header("Inputs (del Canvas de CrearCuenta)")]
    public TMP_InputField usernameInput;     
    public TMP_InputField passwordInput;     
    public TMP_InputField questionInput;     
    public TMP_InputField answerInput;       

    [Header("Texto para mensajes / errores")]
    public TextMeshProUGUI errorText;        

    [Header("Escenas")]
    public string loginSceneName = "IniciarSesion";    

    private DatabaseReference usersRef;

    private void Start()
    {
        TryInitUsersRef();
    }

   
    private void TryInitUsersRef()
    {
        if (usersRef != null) return;

        if (!FirebaseInitializer.IsReady)
        {
            if (errorText != null)
                errorText.text = "Espera un momento, cargando servidor...";
            return;
        }

        try
        {
            usersRef = FirebaseDatabase.DefaultInstance
                                       .RootReference
                                       .Child("users");
            Debug.Log("RegisterController: usersRef inicializado.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("RegisterController: No se pudo inicializar usersRef: " + ex);
            if (errorText != null)
                errorText.text = "No se pudo conectar al servidor.";
        }
    }

    
    public async void OnClickRegister()
    {
        if (errorText != null) errorText.text = "";

        // Asegurarnos de que Firebase está listo
        if (!FirebaseInitializer.IsReady)
        {
            if (errorText != null)
                errorText.text = "Espera un momento, cargando servidor...";
            return;
        }

        
        if (usersRef == null)
        {
            TryInitUsersRef();
            if (usersRef == null)
            {
                if (errorText != null)
                    errorText.text = "Servidor no disponible, intenta de nuevo.";
                return;
            }
        }

        string username = usernameInput.text.Trim();
        string password = passwordInput.text;
        string question = questionInput.text.Trim();
        string answer = answerInput.text.Trim();

        // validacion
        if (string.IsNullOrEmpty(username) || username.Length > 15)
        {
            if (errorText != null)
                errorText.text = "Usuario vacío o muy largo (máx 15).";
            return;
        }

        if (string.IsNullOrEmpty(password) || password.Length < 5 || password.Length > 15)
        {
            if (errorText != null)
                errorText.text = "Contraseña de 5 a 15 caracteres.";
            return;
        }

        if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(answer))
        {
            if (errorText != null)
                errorText.text = "Pregunta y respuesta son obligatorias.";
            return;
        }

        try
        {
            // usuario
            var snapshot = await usersRef.Child(username).GetValueAsync();
            if (snapshot.Exists)
            {
                if (errorText != null)
                    errorText.text = "Ese usuario ya existe.";
                return;
            }

            // hash de la respuesta 
            string passwordHash = CryptoUtils.Sha256(password);
            string answerHash = CryptoUtils.Sha256(answer);

            // Ausuario
            UserData user = new UserData
            {
                username = username,
                passwordHash = passwordHash,
                securityQuestion = question,
                securityAnswerHash = answerHash,
                coins = 0
            };

            // Guardar en Firebase
            string json = JsonUtility.ToJson(user);
            await usersRef.Child(username).SetRawJsonValueAsync(json);

            // Mensaje de exito y redirección
            if (errorText != null)
                errorText.text = "Cuenta creada exitosamente. Redirigiendo a inicio de sesión...";

            StartCoroutine(GoToLoginAfterDelay(1.5f));
        }
        catch (System.Exception ex)
        {
            Debug.LogError("RegisterController: Error al crear cuenta: " + ex);
            if (errorText != null)
                errorText.text = "Error al crear cuenta.";
        }
    }

    
    private System.Collections.IEnumerator GoToLoginAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(loginSceneName);
    }

    
    public void OnClickGoToLogin()
    {
        SceneManager.LoadScene(loginSceneName);
    }
}
