using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Globalization;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Development : MonoBehaviour 
{
    public static Development Instance;

    #region Values
    public bool BuildTextOnly;
    /// <summary>
    /// Determines if output is being shown.
    /// </summary>
    [Space(10)]
    public bool ShowingInputOutput = true;


    private static bool LogOnPrint = true;
    private static bool DebugLogOnPrint = false;

    private static int MaxLogMessagesDisplayed = 8;
    public float BuildInfoVerticalOffset;
    public string BuildText = "PRE-ALPHA BUILD";
    public string BuildDetails;
    public string BuildDate;

    private static List<DebugMessage> messages = new List<DebugMessage>();
    private static List<DisplayedValue> values = new List<DisplayedValue>();
    private static List<Button> buttons = new List<Button>();


    [Space(10)]
    public string[] Notes;
    public string[] Fixes;

    [Space(15)]
    public DevOutputSettings OutputSettings;


    [Space(15)]
    public DevCompositionHelper CompositionHelper;

    [Space(15)]
    private Texture2D whiteTexture;
    private Texture2D redTexture;
    private Texture2D yellowTexture;
    private Texture2D cyanTexture;

    /// <summary>
    /// The action safe screen positions.
    /// </summary>
    float actionSafeTop;
    float actionSafeBottom;
    float actionSafeLeft;
    float actionSafeRight;

    #region FrameRate
    private float accum = 0;
    private int frames = 0;
    private float timeleft;
    private int frameRate;
    #endregion
    private class TimedGizmo
    {
        public enum TimedGizmoType
        {
            Sphere,
            Line
        }
        public float Lifespan;
        public float CreationTime;
        public TimedGizmoType Type;
        public Color Color;
        public float Radius;
        public Vector3 Start;
        public Vector3 End;

        public TimedGizmo(float lifeSpan, Color color, float radius, Vector3 position)
        {
            Type = TimedGizmoType.Sphere;
            Lifespan = lifeSpan;
            CreationTime = Time.time;
            Color = color;
            Radius = radius;
            Start = position;
        }
        public TimedGizmo(float lifeSpan, Color color, Vector3 start, Vector3 end)
        {
            Type = TimedGizmoType.Line;
            Lifespan = lifeSpan;
            CreationTime = Time.time;
            Color = color;
            Start = start;
            End = end;
        }
    }
    private static List<TimedGizmo> _timedGizmos = new List<TimedGizmo>();
    
    private ulong _debugCounter;
    private ulong _debugCounter2 = 0x8534593495347656;
    #endregion

    #region Classes
    [Serializable]
    public enum MessagePriority
    {
        Information,
        Low,
        Medium,
        High
    }
    [Serializable]
    public class DevOutputSettings
    {
        /// <summary>
        /// Defines the font of the gui.
        /// </summary>
        public Font Font;
        public Font BuildFont;
        public Color BuildFontColor = new Color(1, 1, 1, 0.25f);
        /// <summary>
        /// Overrides the alpha of all Output Gui elements
        /// </summary>
        [Range(0, 1)]
        public float GUIAlphaOveride = 0.6f;
        /// <summary>
        /// Defines the default color for output gui elements text
        /// </summary>
        [Space(10)]
        public Color DefaultFontColor = Color.white;
        /// <summary>
        /// Defines the background color of input buttons
        /// </summary>
        public Color ButtonBackgroundColor = Color.black;
        /// <summary>
        /// Defines the text color of notes
        /// </summary>
        [Space(10)]
        public Color NoteColor = Color.yellow;
        [Space(10)]
        public Color BugColor = Color.red;
        /// <summary>
        /// Define the color states of the fps counter
        /// </summary>
        [Space(10)]
        public Color HighFrameRateColor = Color.white;
        public Color LowFrameRateColor = Color.yellow;
        public Color VeryLowFrameRateColor = Color.red;

        // Our log header colors.
        [Space(10)]
        public Color OutHeaderColor = Color.cyan;
        public Color OutTypeInfoColor = Color.grey;
        public Color OutTypeLowColor = Color.yellow;
        public Color OutTypeMediumColor = Color.green;
        public Color OutTypeHighColor = Color.red;


        /// <summary>
        /// Adds to all output input element font sizes
        /// </summary>
        [Space(10)]
        public int FontSizeAdditive = 0;
        [Space(5)]
        public int LogFontSize = 9;
        public int ValueFontSize = 9;
        public int NoteFontSize = 9;
        public int ButtonFontSize = 9;
        public int FpsFontSize = 11;
        public int BuildFontSize = 12;
        public int DateLabelSize = 200;
        public int DateFontSize = 9;
        public int DebugCountersFontSize = 11;

        [HideInInspector]
        public GUIStyle NoteGuiStyle = new GUIStyle();
        [HideInInspector]
        public GUIStyle BugsGuiStyle = new GUIStyle();
        [HideInInspector]
        public GUIStyle ValueGuiStyle = new GUIStyle();
        [HideInInspector]
        public GUIStyle LogGuiStyle = new GUIStyle();
        [HideInInspector]
        public GUIStyle FrameRateGuiStyle = new GUIStyle();
        [HideInInspector]
        public GUIStyle BuildGuiStyle = new GUIStyle();
        [HideInInspector]
        public GUIStyle DateGuiStyle = new GUIStyle();
        [HideInInspector]
        public GUIStyle ButtonGuiStyle = new GUIStyle();
        [HideInInspector]
        public GUIStyle DebugCountersGuiStyle = new GUIStyle();
        [HideInInspector]
        public Texture2D ButtonTexture;

    }
    [Serializable]
    public class DevCompositionHelper
    {
        /// <summary>
        /// Determines if the rule of third markers will be shown.
        /// </summary>
        public bool ShowRuleOfThirds;
        /// <summary>
        /// Determines if the cross point markers will be shown.
        /// </summary>
        public bool ShowCrossPoint;
        /// <summary>
        /// Determines if the safe frame markers will be shown.
        /// </summary>
        public bool ShowSafeFrames;
        /// <summary>
        /// Defines the color of the rule of thirds lines.
        /// </summary>
        [Space(10)]
        public Color RuleOfThirdsColor;
        /// <summary>
        /// Defines the color of the cross-point lines.
        /// </summary>
        public Color CrossPointColor;
        /// <summary>
        /// Defines the color of the action safe lines.
        /// </summary>
        public Color ActionSafeColor;
        /// <summary>
        /// Defines the color of the title safe lines.
        /// </summary>
        public Color TitleSafeColor;
        [HideInInspector]
        public Texture2D ActionSafeTexture;
        [HideInInspector]
        public Texture2D TitleSafeTexture;
        [HideInInspector]
        public Texture2D RuleOfThirdsTexture;
        [HideInInspector]
        public Texture2D CrossPointTexture;
    }
    private class DebugMessage
    {
        public float Duration;
        public string Message;
        public DebugMessage(float duration, string message)
        {
            Duration = duration;
            Message = message;
        }
    }
    private class DisplayedValue
    {
        public string Title;
        public string Message;
        public float Padding;
        public DisplayedValue(string title, string message, float padding)
        {
            Title = title;
            Message = message;
            Padding = padding;
        }
    }

    private class Button
    {
        public string Text;
        public Action Method;
        public object Paramater;
        public Button(string text, Action method, object paramater)
        {
            Text = text;
            Method = method;
            Paramater = paramater;
        }
    }

    #endregion


    #region Unity Functions
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {

        if (Application.platform == RuntimePlatform.XboxOne)
            OutputSettings.FontSizeAdditive += 3;

        #region Color Textures
        OutputSettings.ButtonTexture = new Texture2D(1, 1);
        whiteTexture = new Texture2D(1, 1);
        redTexture = new Texture2D(1, 1);
        yellowTexture = new Texture2D(1, 1);
        cyanTexture = new Texture2D(1, 1);

        CompositionHelper.ActionSafeTexture = new Texture2D(1, 1);
        CompositionHelper.TitleSafeTexture = new Texture2D(1, 1);
        CompositionHelper.RuleOfThirdsTexture = new Texture2D(1, 1);
        CompositionHelper.CrossPointTexture = new Texture2D(1, 1);


        OutputSettings.ButtonTexture.SetPixel(0, 0, OutputSettings.ButtonBackgroundColor);
        whiteTexture.SetPixel(0, 0, new Color(1, 1, 1, 0.25f));
        redTexture.SetPixel(0, 0, Color.red);
        yellowTexture.SetPixel(0, 0, Color.yellow);
        cyanTexture.SetPixel(0, 0, Color.cyan);
        

        CompositionHelper.ActionSafeTexture.SetPixel(0, 0, CompositionHelper.ActionSafeColor);
        CompositionHelper.TitleSafeTexture.SetPixel(0, 0, CompositionHelper.TitleSafeColor);
        CompositionHelper.RuleOfThirdsTexture.SetPixel(0, 0, CompositionHelper.RuleOfThirdsColor);
        CompositionHelper.CrossPointTexture.SetPixel(0, 0, CompositionHelper.CrossPointColor);

        OutputSettings.ButtonTexture.Apply();
        whiteTexture.Apply();
        redTexture.Apply();
        yellowTexture.Apply();
        cyanTexture.Apply();

        CompositionHelper.ActionSafeTexture.Apply();
        CompositionHelper.TitleSafeTexture.Apply();
        CompositionHelper.RuleOfThirdsTexture.Apply();
        CompositionHelper.CrossPointTexture.Apply();
        #endregion

        if (OutputSettings.GUIAlphaOveride > 0)
        {
            OutputSettings.NoteColor.a = OutputSettings.GUIAlphaOveride;
            OutputSettings.DefaultFontColor.a = OutputSettings.GUIAlphaOveride;
            OutputSettings.HighFrameRateColor.a = OutputSettings.GUIAlphaOveride;
            OutputSettings.LowFrameRateColor.a = OutputSettings.GUIAlphaOveride;
            OutputSettings.VeryLowFrameRateColor.a = OutputSettings.GUIAlphaOveride;
            OutputSettings.ButtonBackgroundColor.a = OutputSettings.GUIAlphaOveride; 
        }

        #region Gui Styles

        #region Notes
        OutputSettings.NoteGuiStyle.alignment = TextAnchor.MiddleRight;

        OutputSettings.NoteGuiStyle.font = OutputSettings.Font;
        OutputSettings.NoteGuiStyle.fontSize = (OutputSettings.NoteFontSize + OutputSettings.FontSizeAdditive);
        OutputSettings.NoteGuiStyle.alignment = TextAnchor.UpperLeft;
        OutputSettings.NoteGuiStyle.normal.textColor = OutputSettings.NoteColor;
        #endregion
        #region Bugs
        OutputSettings.BugsGuiStyle.alignment = TextAnchor.MiddleRight;

        OutputSettings.BugsGuiStyle.font = OutputSettings.Font;
        OutputSettings.BugsGuiStyle.fontSize = (OutputSettings.NoteFontSize + OutputSettings.FontSizeAdditive);
        OutputSettings.BugsGuiStyle.alignment = TextAnchor.UpperLeft;
        OutputSettings.BugsGuiStyle.normal.textColor = OutputSettings.BugColor;
        #endregion
        
        #region Values
        OutputSettings.ValueGuiStyle.normal.textColor = OutputSettings.DefaultFontColor;
        OutputSettings.ValueGuiStyle.alignment = TextAnchor.UpperRight;
        OutputSettings.ValueGuiStyle.font = OutputSettings.Font;
        OutputSettings.ValueGuiStyle.fontSize = (OutputSettings.ValueFontSize + OutputSettings.FontSizeAdditive);
        #endregion
        #region Logs
        OutputSettings.LogGuiStyle.normal.textColor = OutputSettings.DefaultFontColor;
        OutputSettings.LogGuiStyle.alignment = TextAnchor.LowerLeft;
        OutputSettings.LogGuiStyle.font = OutputSettings.Font;
        OutputSettings.LogGuiStyle.fontSize = (OutputSettings.LogFontSize + OutputSettings.FontSizeAdditive);
        OutputSettings.LogGuiStyle.richText = true;
        #endregion
        #region FPS Counter
        OutputSettings.FrameRateGuiStyle.normal.textColor = OutputSettings.DefaultFontColor;
        OutputSettings.FrameRateGuiStyle.alignment = TextAnchor.LowerLeft;
        OutputSettings.FrameRateGuiStyle.font = OutputSettings.Font;
        OutputSettings.FrameRateGuiStyle.fontSize = (OutputSettings.FpsFontSize + OutputSettings.FontSizeAdditive);
        #endregion
        #region Build Text
        OutputSettings.BuildGuiStyle.alignment = TextAnchor.LowerCenter;
        OutputSettings.BuildGuiStyle.normal.textColor = OutputSettings.BuildFontColor;
        OutputSettings.BuildGuiStyle.font = OutputSettings.BuildFont;
        OutputSettings.BuildGuiStyle.fontSize = (OutputSettings.BuildFontSize + OutputSettings.FontSizeAdditive);
        #endregion
        #region Date Text
        OutputSettings.DateGuiStyle.alignment = TextAnchor.LowerCenter;
        OutputSettings.DateGuiStyle.normal.textColor = OutputSettings.BuildFontColor;
        OutputSettings.DateGuiStyle.font = OutputSettings.BuildFont;
        OutputSettings.DateGuiStyle.fontSize = (OutputSettings.DateFontSize + OutputSettings.FontSizeAdditive);
        #endregion
        #region Button
        OutputSettings.ButtonGuiStyle.fontSize = (OutputSettings.ButtonFontSize + OutputSettings.FontSizeAdditive);
        OutputSettings.ButtonGuiStyle.normal.textColor = Color.white;
        OutputSettings.ButtonGuiStyle.font = OutputSettings.Font;
        OutputSettings.ButtonGuiStyle.alignment = TextAnchor.MiddleCenter;
        #endregion
        #region DebugCounter
        OutputSettings.DebugCountersGuiStyle.alignment = TextAnchor.LowerRight;
        OutputSettings.DebugCountersGuiStyle.normal.textColor = OutputSettings.BuildFontColor;
        OutputSettings.DebugCountersGuiStyle.font = OutputSettings.BuildFont;
        OutputSettings.DebugCountersGuiStyle.fontSize = (OutputSettings.DebugCountersFontSize + OutputSettings.FontSizeAdditive);
        #endregion
        #endregion

    }

    private void Update()
    {
        // Define action safe bounds
        actionSafeTop = (Screen.height - (Screen.height * 0.9f)) / 2;
        actionSafeBottom = Screen.height * 0.95f;
        actionSafeLeft = Screen.width - (Screen.width * 0.95f);
        actionSafeRight = (Screen.width * 0.95f);

        UpdateFrameRate();


        if (BuildTextOnly)
            return;

        UpdateMessageDurations();
    }

    private void OnGUI()
    {
        DisplayBuildText();
        DisplayFrameRate();

        if (BuildTextOnly)
            return;


        DisplayCompositionHelpers();
        DisplayDebugCounters();
        
        if (ShowingInputOutput)
        {
            if (Application.platform != RuntimePlatform.XboxOne)
                DisplayButtons();

            DisplayValues();
            DisplayLogs();
            DisplayNotesAndFixes();
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < _timedGizmos.Count; i++)
        {
            TimedGizmo timedGizmo = _timedGizmos[i];
            if (timedGizmo.Lifespan + timedGizmo.CreationTime < Time.time)
            {
                _timedGizmos.Remove(timedGizmo);
                i--;
                continue;
            }

            if (timedGizmo.Type == TimedGizmo.TimedGizmoType.Sphere)
            {
                Gizmos.color = timedGizmo.Color;
                Gizmos.DrawWireSphere(timedGizmo.Start, timedGizmo.Radius);
            }
            else
                Debug.DrawLine(timedGizmo.Start, timedGizmo.End, timedGizmo.Color);
        }
    }
    private void OnDisable()
    {
        _timedGizmos.Clear();
    }
    private void OnDistroy()
    {
        _timedGizmos.Clear();
    }
    #endregion

    #region Functions
    public static void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public static void ClearConsole()
    {
        Type logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
        MethodInfo clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }

    public static void AddTimedLineGizmo(Vector3 start, Vector3 end, Color color, float lifeSpan = 0)
    {
        _timedGizmos.Add(new TimedGizmo(lifeSpan, color, start, end));
    }
    public static void AddTimedSphereGizmo(Color color, float radius, Vector3 position, float lifeSpan = 0)
    {
        _timedGizmos.Add(new TimedGizmo(lifeSpan, color, radius, position));
    }


    public static void ShowButton(string text, Action method)
    {
        foreach (Button button in buttons)
        {
            if (button.Text == text)
            {
                button.Method = method;
                return;
            }
        }
        Button newButton = new Button(text, method, null);
        buttons.Add(newButton);
    }

    public static void ShowValue(string title, object message)
    {
        if (Instance == null)
            return;

        foreach (DisplayedValue valueDisplay in values)
        {
            if (valueDisplay.Title == title)
            {
                valueDisplay.Message = message.ToString();
                return;
            }
        }
        DisplayedValue newValueDisplay = new DisplayedValue(title, message.ToString(), 0);
        values.Add(newValueDisplay);
    }
    public static void ShowValue(string title, object message, float topPadding)
    {
        foreach (DisplayedValue valueDisplay in values)
        {
            if (valueDisplay.Title == title)
            {
                valueDisplay.Message = message.ToString();
                return;
            }
        }
        DisplayedValue newValueDisplay = new DisplayedValue(title, message.ToString(), topPadding);
        values.Add(newValueDisplay);
    }

    public static void Log(object message)
    {
        Log(message, 5);
    }
    public static void Log(object message, float duration)
    {
        Log(message, string.Empty, duration);
    }
    public static void Log(object message, string objectName, float duration)
    {
        string compiledMessage = message.ToString();
        if (!string.IsNullOrEmpty(objectName))
            compiledMessage += " (" + objectName + ")";
        DebugMessage newMessage = new DebugMessage(duration, compiledMessage);
        messages.Insert(0, newMessage);
    }

    public static void Out(object source, string message, MessagePriority messagePriority)
    {
        // If we're not outputting, stop.
        if (!DebugLogOnPrint && !LogOnPrint)
            return;
        
        // Determine our name from type.
        string name = source.GetType().ToString();

        // Print our message
        Color colorPriority = Instance.OutputSettings.OutTypeInfoColor;
        switch (messagePriority)
        {
            case MessagePriority.Information:
                colorPriority = Instance.OutputSettings.OutTypeInfoColor;
                break;
            case MessagePriority.Low:
                colorPriority = Instance.OutputSettings.OutTypeLowColor;
                break;
            case MessagePriority.Medium:
                colorPriority = Instance.OutputSettings.OutTypeMediumColor;
                break;
            case MessagePriority.High:
                colorPriority = Instance.OutputSettings.OutTypeHighColor;
                break;
        }
        Out(string.Format("<color='#{0}'>[{1}]: </color><color='#{2}'>{3}</color>", Instance.OutputSettings.OutHeaderColor.GetHashCode(), name, colorPriority.GetHashCode(), message));
    }
    private static void Out(string message)
    {
        if(DebugLogOnPrint)
            Debug.Log(message);
        if (LogOnPrint)
            Log(message);
    }
    #endregion

    #region Display
    private void DisplayLogs()
    {
        if (messages.Count == 0)
            return;

        // Get messages in range of max display count
        List<DebugMessage> existingMessages;
        if (messages.Count > MaxLogMessagesDisplayed)
            existingMessages = messages.GetRange(0, MaxLogMessagesDisplayed);
        else
            existingMessages = messages;

        // Print logs to gui
        for (int m = 0; m < existingMessages.Count; m++)
        {
            string message = existingMessages[m].Message;
            if (message == string.Empty)
                message = "...";

            GUI.Label(new Rect(new Vector2(actionSafeLeft, (actionSafeBottom - 80) + (-15 * m)), new Vector2(800, 50)), message, OutputSettings.LogGuiStyle);
        }
    }
    private void DisplayValues()
    {
        float valuePositon = actionSafeTop - 15;

        // Display screen logs
        for (int m = 0; m < values.Count; m++)
        {
            valuePositon += (15 + values[m].Padding);
            GUI.Label(new Rect(new Vector2(actionSafeRight - 800, valuePositon), new Vector2(800, 50)), values[m].Title + " " + values[m].Message, OutputSettings.ValueGuiStyle);
        }
    }
    private void DisplayNotesAndFixes()
    {
        float valuePositon = actionSafeTop - 15;
        if (Fixes.Length > 0)
        {
            valuePositon += 15;
            GUI.Label(new Rect(new Vector2(actionSafeLeft, valuePositon), new Vector2(800, 50)), "FIXES:", OutputSettings.BugsGuiStyle);
            for (int b = 0; b < Fixes.Length; b++)
            {
                if (Fixes[b] == string.Empty)
                    continue;
                valuePositon += 15;
                GUI.Label(new Rect(new Vector2(actionSafeLeft, valuePositon), new Vector2(800, 50)), Fixes[b], OutputSettings.BugsGuiStyle);
            }
        }
        if (Notes.Length > 0)
        {
            valuePositon += Fixes.Length > 0 ? 30 : 15;
            GUI.Label(new Rect(new Vector2(actionSafeLeft, valuePositon), new Vector2(800, 50)), "NOTES:", OutputSettings.NoteGuiStyle);

            for (int n = 0; n < Notes.Length; n++)
            {
                if (Notes[n] == string.Empty)
                    continue;
                valuePositon += 15;
                GUI.Label(new Rect(new Vector2(actionSafeLeft, valuePositon), new Vector2(800, 50)), Notes[n], OutputSettings.NoteGuiStyle);
            }
        }
    }
    private void DisplayButtons()
    {
        if (buttons.Count == 0)
            return;

        // Display screen buttons
        for (int m = 0; m < buttons.Count; m++)
        {
            // Black Background
            GUI.DrawTexture(new Rect(actionSafeLeft, actionSafeTop + (30 * m), 120, 20), OutputSettings.ButtonTexture);
            // Button
            if (GUI.Button(new Rect(actionSafeLeft, actionSafeTop + (30 * m), 120, 20), buttons[m].Text, OutputSettings.ButtonGuiStyle))
            {
                buttons[m].Method.Invoke();
            }
        }
    }
    private void DisplayFrameRate()
    {
        // Determine the color of the counter
        OutputSettings.FrameRateGuiStyle.normal.textColor = (frameRate >= 50) ? OutputSettings.HighFrameRateColor : ((frameRate > 30) ? OutputSettings.LowFrameRateColor : OutputSettings.VeryLowFrameRateColor);
        GUI.Label(new Rect(new Vector2(actionSafeLeft, actionSafeBottom - 50), new Vector2(800, 50)), "[ " + frameRate + " ]", OutputSettings.FrameRateGuiStyle);
    }
    private void DisplayBuildText()
    {
        BuildText = BuildText.ToUpper();

        BuildDetails = BuildDetails.Replace(" ", "_");
        BuildDetails = BuildDetails.ToUpper();

        //        string platform = "UNDEFINED";
        //#if UNITY_EDITOR
        //        platform = "EDITOR";
        //#elif UNITY_STANDALONE_WIN
        //        platform = "WIN";
        //#elif UNITY_STANDALONE_OSX
        //        platform = "OSX";
        //#elif UNITY_STANDALONE_LINUX
        //        platform = "UNIX";
        //#elif UNITY_XBOXONE
        //        platform = "XBOX";
        //#endif


        //Display alpha build text at the bottom center of the screen
        GUI.Label(new Rect(new Vector2((Screen.width / 2) - 200, actionSafeBottom - BuildInfoVerticalOffset - 12), new Vector2(400, 50)), BuildText, OutputSettings.BuildGuiStyle);

        GUI.Label(new Rect(new Vector2((Screen.width / 2) - 200, actionSafeBottom - BuildInfoVerticalOffset), new Vector2(400, 50)), BuildDetails + "_" + BuildDate, OutputSettings.DateGuiStyle);
    }

    private void DisplayDebugCounters()
    {
        _debugCounter += 0x00723345654345A3;
        _debugCounter2 += 0x00723345654345A3;
        //Display alpha build text at the bottom center of the screen
        GUI.Label(new Rect(new Vector2(actionSafeRight - 400, actionSafeBottom - 50), new Vector2(400, 50)), _debugCounter.ToString("X16"), OutputSettings.DebugCountersGuiStyle);
        GUI.Label(new Rect(new Vector2(actionSafeRight - 400, actionSafeBottom - 65), new Vector2(400, 50)), _debugCounter2.ToString("X16"), OutputSettings.DebugCountersGuiStyle);
    }

    private void DisplayCompositionHelpers()
    {

        // TODO: Add horizontal reticle offset
        float reticleOffset = 0;

        #region Safe Frames
        if (CompositionHelper.ShowSafeFrames)
        {
            // Action safe Vertical
            GUI.DrawTexture(new Rect(actionSafeLeft, actionSafeTop + reticleOffset, 1, (Screen.height * 0.9f)), CompositionHelper.ActionSafeTexture);
            GUI.DrawTexture(new Rect(actionSafeRight, actionSafeTop + reticleOffset, 1, (Screen.height * 0.9f)), CompositionHelper.ActionSafeTexture);
            // Horizontal
            GUI.DrawTexture(new Rect(actionSafeLeft, actionSafeTop + reticleOffset, (Screen.width * 0.9f), 1), CompositionHelper.ActionSafeTexture);
            GUI.DrawTexture(new Rect(actionSafeLeft, actionSafeBottom + reticleOffset, (Screen.width * 0.9f), 1), CompositionHelper.ActionSafeTexture);

            // Title Safe Vertical
            GUI.DrawTexture(new Rect(Screen.width - (Screen.width * 0.9f), (Screen.height - (Screen.height * 0.8f)) / 2 + reticleOffset, 1, (Screen.height * 0.8f)), CompositionHelper.TitleSafeTexture);
            GUI.DrawTexture(new Rect((Screen.width * 0.9f), (Screen.height - (Screen.height * 0.8f)) / 2 + reticleOffset, 1, (Screen.height * 0.8f)), CompositionHelper.TitleSafeTexture);
            // Horizontal
            GUI.DrawTexture(new Rect(Screen.width - (Screen.width * 0.9f), (Screen.height - (Screen.height * 0.8f)) / 2 + reticleOffset, (Screen.width * 0.8f), 1), CompositionHelper.TitleSafeTexture);
            GUI.DrawTexture(new Rect(Screen.width - (Screen.width * 0.9f), Screen.height * 0.9f + reticleOffset, (Screen.width * 0.8f), 1), CompositionHelper.TitleSafeTexture);
        }
        #endregion


        // Rule of Thirds
        if (CompositionHelper.ShowRuleOfThirds)
            for (int i = 1; i < 3; i++)
            {
                GUI.DrawTexture(new Rect(0, ((Screen.height / 3) * i) + reticleOffset, Screen.width, 1), CompositionHelper.RuleOfThirdsTexture);

                GUI.DrawTexture(new Rect((Screen.width / 3) * i, 0, 1, Screen.height), CompositionHelper.RuleOfThirdsTexture);
            }

        // Cross point
        if (CompositionHelper.ShowCrossPoint)
        {
            GUI.DrawTexture(new Rect(0, (Screen.height / 2) + reticleOffset, Screen.width, 1), CompositionHelper.CrossPointTexture);
            GUI.DrawTexture(new Rect((Screen.width / 2), 0, 1, Screen.height ), CompositionHelper.CrossPointTexture);
        }
    }
    #endregion

    #region Update Functions
    private void UpdateFrameRate()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        if (timeleft <= 0.0)
        {
            float fps = accum / frames;

            if (0 > (int)fps)
                fps = 0;

            frameRate = (int)fps;


            timeleft = 0.1F;
            accum = 0;
            frames = 0;
        }
    }

    private void UpdateMessageDurations()
    {
        List<DebugMessage> frameMessages = new List<DebugMessage>();
        frameMessages.AddRange(messages);

        foreach (DebugMessage message in frameMessages)
        {
            message.Duration -= Time.deltaTime;
            if (message.Duration <= 0)
                messages.Remove(message);
        }
    }
    #endregion
    
}