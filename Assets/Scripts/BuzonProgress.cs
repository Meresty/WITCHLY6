using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public static class BuzonProgress
{
    [Serializable]
    private class StringListWrapper
    {
        public List<string> items = new List<string>();
    }

    private const string KEY_CREATED = "BUZON_POCIONES_CREADAS";

    public static void UnlockPotion(string pocionNombre)
    {
        string key = NormalizeKey(pocionNombre);
        if (string.IsNullOrEmpty(key)) return;

        var list = Load();
        if (!list.items.Contains(key))
        {
            list.items.Add(key);
            Save(list);
        }
    }

    // ✅ NUEVO: se usa al ENTREGAR una pocion (se consume y se vuelve a bloquear)
    public static void ConsumePotion(string pocionNombre)
    {
        string key = NormalizeKey(pocionNombre);
        if (string.IsNullOrEmpty(key)) return;

        var list = Load();
        if (list.items.Contains(key))
        {
            list.items.Remove(key);
            Save(list);
        }
    }

    public static bool IsUnlocked(string pocionNombre)
    {
        string key = NormalizeKey(pocionNombre);
        if (string.IsNullOrEmpty(key)) return false;

        var list = Load();
        return list.items.Contains(key);
    }

    private static StringListWrapper Load()
    {
        try
        {
            if (!PlayerPrefs.HasKey(KEY_CREATED)) return new StringListWrapper();
            var json = PlayerPrefs.GetString(KEY_CREATED, "");
            if (string.IsNullOrWhiteSpace(json)) return new StringListWrapper();

            var data = JsonUtility.FromJson<StringListWrapper>(json);
            return (data != null && data.items != null) ? data : new StringListWrapper();
        }
        catch
        {
            return new StringListWrapper();
        }
    }

    private static void Save(StringListWrapper data)
    {
        try
        {
            PlayerPrefs.SetString(KEY_CREATED, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
        catch { }
    }

    private static string NormalizeKey(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        s = s.Trim().ToLowerInvariant();

        string formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);

        for (int i = 0; i < formD.Length; i++)
        {
            char ch = formD[i];
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }

        s = sb.ToString().Normalize(NormalizationForm.FormC);

        s = s.Replace(" ", "")
             .Replace("_", "")
             .Replace("-", "")
             .Replace("(clone)", "");

        return s;
    }
}
