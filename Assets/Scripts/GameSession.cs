using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }
    public UserData CurrentUser { get; private set; }

    public string Username => CurrentUser != null ? CurrentUser.username : "";

    private void Awake()
    {
        Debug.Log("GameSession.Awake");
        
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetUser(UserData user)
    {
        CurrentUser = user;

        // Bind de servicios Firebase a este usuario
        if (FirebaseCoinsManager.Instance != null)
            FirebaseCoinsManager.Instance.BindUser(user.username);

        if (FirebaseInventoryManager.Instance != null)
            FirebaseInventoryManager.Instance.BindUser(user.username);
    }

    public void ClearUser()
    {
        CurrentUser = null;

        if (FirebaseCoinsManager.Instance != null)
            FirebaseCoinsManager.Instance.Unbind();

        if (FirebaseInventoryManager.Instance != null)
            FirebaseInventoryManager.Instance.Unbind();
    }
}
