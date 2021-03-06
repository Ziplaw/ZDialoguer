using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ZDialoguer.Localization;

public class LocalizationSettings : ScriptableObject
{
    public static LocalizationSettings Instance => _instance ??= Resources.Load<LocalizationSettings>("Localization Settings");
    private static LocalizationSettings _instance;
    public char separator;
    public List<string> languages;
    public int selectedLanguage;

    internal Action<string> OnLanguageChange;

    public void SetLanguage(string languageName)
    {
        selectedLanguage = languages.IndexOf(languageName);
        OnLanguageChange?.Invoke(languageName);
    }
}
