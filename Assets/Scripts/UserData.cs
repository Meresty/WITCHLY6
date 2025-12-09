using System;

[Serializable]
public class UserData
{
    public string username;
    public string passwordHash;
    public string securityQuestion;
    public string securityAnswerHash;
    public int coins;
}
