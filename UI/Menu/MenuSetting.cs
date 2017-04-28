using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;

public class MenuSetting : MenuControl
{
    #region Values

    public enum NetworkSettingType
    {
        None,
        GameSetting,
    }
    public NetworkSettingType NetworkSetting;

    /// <summary>
    /// Defines the text displaying the current selected setting option
    /// </summary>
    [Space(15)]
    public Text SettingText;

    /// <summary>
    /// Defines the animation set for the left bumper.
    /// </summary>
    public UIAnimationSet LeftBumperAnimationSet;
    /// <summary>
    /// Defines the animation set for the right bumper.
    /// </summary>
    public UIAnimationSet RightBumperAnimationSet;
    /// <summary>
    /// Defines the name of the animation for both the right and left bumper.
    /// </summary>
    public string BumperChangeAnimationName;


    /// <summary>
    /// Defines that path of the value to be changed.
    /// </summary>
    [Space(15)]
    public string ValuePath;

    [Serializable]
    public class ValueStringReplacement
    {
        public bool ReplacePart;
        public string Case;
        public string Replacement;
    }
    /// <summary>
    /// A collection of value string replacement cases.
    /// </summary>
    [Space(5)]
    public ValueStringReplacement[] ValueStringReplacements;
    /// <summary>
    /// Determines if each capital letter will have a space added to its left.
    /// </summary>
    public bool SpaceValueStringCamelCase;
    /// <summary>
    /// Defines the format of the value string.
    /// </summary>
    public string ValueStringFormat;

    public enum TextCaseType
    {
        None,
        ToLower,
        ToUpper
    }
    /// <summary>
    /// Determines the case of the value string.
    /// </summary>
    public TextCaseType ValueStringCase;

    [Serializable]
    public class ValueCase
    {
        public string Name;
        public int Case;
        [Serializable]
        public class ChildSetting
        {
            [Header("Child Setting")]
            public MenuSetting AffectedSetting;
            public bool UseNewValue;
            public int AffectedSettingNewValue;
            public bool Locked;

            [Space(10)]
            [Header("Game Object State Change")]
            public GameObject GameObjectStateChange;
            public bool State;
        }
        public ChildSetting[] ChildSettings;
    }
    public ValueCase[] ValueCases;
    #endregion

    #region Functions
    public override void Load(MenuControlGroup parent = null, bool changed = false)
    {
        if (parent != null)
            base.Load(parent);

        string path;
        object root;
        GetPathInfo(out root, out path, ValuePath);

        // Define value
        object value = ObjectReferencer.GetValue(root, path);
        if (value == null)
            return;

        // Update child settings
            if (value.GetType() == typeof(System.Boolean))
                SetChildSettings((bool)value ? 1 : 0, changed);
            else if (value.GetType().BaseType == typeof(System.Enum))
                SetChildSettings((int)value, changed);

        // Define the string of the value
        string valueString = value.ToString();
        valueString = AlterTextCase(valueString);

        // Set text
        SettingText.text = SetTextState(ValueStringCase, valueString);

    }

    public void Change(bool forward)
    {
        if (Locked || ValuePath == string.Empty)
            return;

        // Define path and root
        string path;
        object root;
        GetPathInfo(out root, out path, ValuePath);

        string[] settingDirectories = path.Split('/');
        string settingName = settingDirectories[settingDirectories.Length - 1];

        // Define value
        object currentValue = ObjectReferencer.GetValue(root, path);
        if (currentValue == null)
            return;
        // Determine if the value is a bool, if so toggle it
        if (currentValue.GetType() == typeof(System.Boolean))
        {
            ObjectReferencer.SetValue(root, !(bool)currentValue, path);
            ChangeSettingForNetwork(settingName, !(bool)currentValue == false ? 0 : 1);
        }
        else if (currentValue.GetType().BaseType == typeof(System.Enum))
        {
            // Otherwise if the value is an enum, advance
            int length = Enum.GetNames(currentValue.GetType()).Length;
            int value = (int)currentValue;
            value = forward ? value + 1 : value - 1;

            // If the value is smaller than zero, define it as the maximum value of the enum
            if (value < 0)
                value = length - 1;
            else if (value > length - 1)
                // Otherwise if the value is larger than the maximum value, define it as zero
                value = 0;

            // Set the value
            ObjectReferencer.SetValue(root, value, path);

            ChangeSettingForNetwork(settingName, value);
        }
        Load(null, true);
        Animate(forward);
    }
    
    public void RemoteValueSet(int value)
    {
        string path;
        object root;
        GetPathInfo(out root, out path, ValuePath);
        object currentValue = ObjectReferencer.GetValue(root, path);

        string[] settingDirectories = path.Split('/');
        string settingName = settingDirectories[settingDirectories.Length - 1];

        ChangeSettingForNetwork(settingName, value);

        if (currentValue.GetType() == typeof(System.Boolean))
            ObjectReferencer.SetValue(root, value == 1, path);
        else
            ObjectReferencer.SetValue(root, value, path);
        Load();
    }

    private void SetChildSettings(int value, bool changed)
    {
        foreach (ValueCase valueCase in ValueCases)
            if (value == valueCase.Case)
                foreach (ValueCase.ChildSetting child in valueCase.ChildSettings)
                {
                    if (child.AffectedSetting != null)
                    {
                        child.AffectedSetting.Locked = child.Locked;
                        if (child.UseNewValue && changed)
                            child.AffectedSetting.RemoteValueSet(child.AffectedSettingNewValue);
                    }

                    if (child.GameObjectStateChange != null)
                    {
                        child.GameObjectStateChange.SetActive(child.State);
                    }
                }
    }

    private void Animate(bool forward)
    {
        UIAnimationSet uiAnimationSet = forward ? RightBumperAnimationSet : LeftBumperAnimationSet;
        uiAnimationSet.Play(BumperChangeAnimationName);
    }

    private string SetTextState(TextCaseType textCaseType, string text)
    {
        string result = text;
        switch (textCaseType)
        {
            case TextCaseType.None:
                break;
            case TextCaseType.ToLower:
                result = result.ToLower();
                break;
            case TextCaseType.ToUpper:
                result = result.ToUpper();
                break;
        }
        return result;
    }
    private string AlterTextCase(string text)
    {
        foreach (ValueStringReplacement valueStringReplacement in ValueStringReplacements)
        {
            // If we are to replace part of the string, look for the case, and replace if found
            if (valueStringReplacement.ReplacePart)
                text = text.Replace(valueStringReplacement.Case, valueStringReplacement.Replacement);
            else
                // Otherwise if the string is exact replace the entire string
                text = text == valueStringReplacement.Case ? valueStringReplacement.Replacement : text;
        }

        // If we are to space camel case strings insert a space before each capital letter
        if (SpaceValueStringCamelCase)
            text = Regex.Replace(text, "(\\B[A-Z])", " $1");

        // Format the string
        if (ValueStringFormat != string.Empty)
            text = String.Format(ValueStringFormat, text);

        return text;
    }

    public void ChangeSettingForNetwork(string setting, int value)
    {
        if (!NetworkSessionManager.IsHost)
            return;

        switch (NetworkSetting)
        {
            case NetworkSettingType.GameSetting:
                NetworkSessionNode.Instance.RpcChangeGameSettingForClients(setting, value);
                break;
        }
    }
    #endregion
}
