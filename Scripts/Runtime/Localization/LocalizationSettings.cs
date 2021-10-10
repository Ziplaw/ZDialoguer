using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LocalizationSettings : ScriptableObject
{
    public static LocalizationSettings Instance => _instance ??= Resources.Load<LocalizationSettings>("Localization Settings");
    private static LocalizationSettings _instance;
    public char separator;
    public List<string> languages;
    public int selectedLanguage;

    public void SetLanguage(string languageName)
    {
        selectedLanguage = languages.IndexOf(languageName);
    }
}
