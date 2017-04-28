using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

public class HeadsUpDisplay : MonoBehaviour
{
    #region Values
    [HideInInspector]
    public LocalPlayer LocalPlayer;
    
    /// <summary>
    /// Defines the default width of the heads up display
    /// </summary>
    public float ReferenceWidth;

    // Defines the screen size used for the last rect appropriation.
    private Vector2 _activeHudSize;

    /// <summary>
    /// Defines the scale factor of the hud compared to the reference width.
    /// </summary>
    private float _scaleFactor;
    public float ScaleFactor
    {
        get { return _scaleFactor; }
        set
        {
            _scaleFactor = value;
            _canvasScaler.scaleFactor = _scaleFactor;
        }
    }

    /// <summary>
    /// Defines the text componenet of the feed.
    /// </summary>
    public Text FeedText;

    /// <summary>
    /// Defines the containing object of the heads up display
    /// </summary>
    [Space(15)]
    public RectTransform HudContainer;
    /// <summary>
    /// Defines the containing object of damage indicators
    /// </summary>
    public RectTransform PointerContainer;
    /// <summary>
    /// Defines the containing object of waypoints
    /// </summary>
    public RectTransform WaypointsContainer;
    /// <summary>
    /// Defines the containing object of waypoints
    /// </summary>
    public RectTransform EmojisContainer;

    /// <summary>
    /// Defines the damage indicator prefab.
    /// </summary>
    [Space(10)]
    public GameObject DamagePointerPrefab;
    /// <summary>
    /// Defines the alert indicator prefab.
    /// </summary>
    public GameObject AlertPointerPrefab;


    // Pointers
    [Serializable]
    public class Pointer
    {
        public enum Type
        {
            Damage,
            Alert
        }
        /// <summary>
        /// Defines originating position of the damage. (Used only if player is null)
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Defines the rect transform of the pointer ui object.
        /// </summary>
        public RectTransform RectTransform;

        public Pointer(Vector3 position, RectTransform rectTransform)
        {
            Position = position;
            RectTransform = rectTransform;
        }
    }
    [HideInInspector]
    private List<Pointer> _pointers = new List<Pointer>();

    // Waypoints
    [Serializable]
    public class Waypoint
    {
        /// <summary>
        /// Determine what type the waypoint is.
        /// </summary>
        public enum WaypointType
        {
            TeamMate,
            ObjectiveCapture,
            ObjectiveKOTH,
            ObjectiveArm,
            ObjectiveBomb,
            AlertAttached,
            Enemy,
            Beacon
        }

        public WaypointType Type;
        /// <summary>
        /// Determines if the waypoint is active.
        /// </summary>
        public bool Active;
        /// <summary>
        /// Defines the transform of the object the waypoint is slave to.
        /// </summary>
        public Transform Transform;
        /// <summary>
        /// Defines the rect transform of the waypoint.
        /// </summary>
        public RectTransform RectTransform;
        /// <summary>
        /// Defines the offset per axis in-which the waypoint will rest from the transforms position.
        /// </summary>
        public Vector3 WorldOffset;

        public Waypoint(WaypointType type, Transform transform, RectTransform rectTransform, Vector3 worldOffset)
        {
            Type = type;
            Active = true;
            Transform = transform;
            RectTransform = rectTransform;
            WorldOffset = worldOffset;
        }
    }
    [HideInInspector]
    private List<Waypoint> _waypoints = new List<Waypoint>();

    public class Emoji
    {
        /// <summary>
        /// Determines if the emoji is active.
        /// </summary>
        public bool Active;
        /// <summary>
        /// Defines the type of the emoji.
        /// </summary>
        public byte Type;
        /// <summary>
        /// Defines the from text of the emoji.
        /// </summary>
        public string Text;
        /// <summary>
        /// Defines the rect transform of the emoji.
        /// </summary>
        public RectTransform RectTransform;

        public Emoji(byte type, string fromName)
        {
            Type = type;
            Text = fromName;
        }
    }
    private List<Emoji> _emojis = new List<Emoji>();
    [Serializable]
    public class EmojiPrefabSettings
    {
        public GameObject Emoji;
    }
    public EmojiPrefabSettings[] EmojiPrefabs;

    // Components
    [Serializable]
    public class Component
    {
        /// <summary>
        /// Defines the name of the component.
        /// </summary>
        public string Name;

        /// <summary>
        /// Defines the text component associated with the hud component.
        /// </summary>
        [HideInInspector]
        public Text Text;
        /// <summary>
        /// Defines the maskable graphic component associated with the hud component.
        /// </summary>
        public MaskableGraphic MaskableGraphic;

        public bool InstantiateMaterial;


        [Serializable]
        public class ValueCaseSet
        {
            public string ValuePath;
            /// <summary>
            /// Defines the maskable graphic's material property adjusted by the color.
            /// </summary>
            [Serializable]
            public class ValueCase
            {
                /// <summary>
                /// Defines the state in-which this color will be applied to the maskable graphic.
                /// </summary>
                public string Case;
                [Serializable]
                public class CaseVisibility
                {
                    public bool Use;
                    public bool Visible;
                }
                public CaseVisibility Visibility;

                [Serializable]
                public class CaseColor
                {
                    public bool Use;
                    /// <summary>
                    /// Defines the color that is to be applied to the maskable graphic.
                    /// </summary>
                    public Color Color;
                    public string MaterialColorPropertyName;
                }
                public CaseColor Color;

                [Serializable]
                public class CaseAnimation
                {
                    public bool Use;
                    [HideInInspector]
                    public bool ActiveCaseState;
                    public UIAnimationSet UIAnimationSet;
                    public bool AllowReplay;
                    public string AnimationName;
                }
                public CaseAnimation Animation;
            }
            /// <summary>
            /// Defines the color of the maskable graphic upon a case.
            /// </summary>
            public ValueCase[] Cases;
        }
        [Space(10)]
        public ValueCaseSet[] ValueCaseSets;

        [Serializable]
        public class HudEvent
        {
            public string Event;
            [Serializable]
            public class CaseVisibility
            {
                public bool Use;
                public bool Visible;
            }
            public CaseVisibility Visibility;

            [Serializable]
            public class CaseColor
            {
                public bool Use;
                /// <summary>
                /// Defines the color that is to be applied to the maskable graphic.
                /// </summary>
                public Color Color;
                public string MaterialColorPropertyName;
            }
            public CaseColor Color;

            [Serializable]
            public class CaseAnimation
            {
                /// <summary>
                /// Determines if the animation is to be used.
                /// </summary>
                public bool Use;
                /// <summary>
                /// Defines the animation set of the animation.
                /// </summary>
                public UIAnimationSet UIAnimationSet;
                /// <summary>
                /// Defines the name of the animation.
                /// </summary>
                public string AnimationName;
                /// <summary>
                /// Defines if the animation is allowed to be replayed while active.
                /// </summary>
                public bool AllowReplay;
            }
            public CaseAnimation Animation;

        }
        [Space(10)]
        public HudEvent[] Events;

        [Serializable]
        public class TextValue
        {
            /// <summary>
            /// Defines the string paths used to determine the text of the value applied to the text component.
            /// </summary>
            public string[] TextValuePaths;
            /// <summary>
            /// Determines the the format of the value collective value string. 
            /// </summary>
            public string TextValueFormat;
            /// <summary>
            /// Defines a string that is to be omitted from the text value.
            /// </summary>
            public string TextValueOmmision;

            public enum TextModType
            {
                None,
                Ceiling,
                Floor,
                TimeSpanMSS,
                AddPlusToPositive
            }
            /// <summary>
            /// Determines the modification that is to be applied to the text value.
            /// </summary>
            public TextModType TextValueMod;
            public enum TextCaseType
            {
                None,
                ToLower,
                ToUpper
            }
            public TextCaseType TextValueCase;
        }
        [Space(10)]
        public TextValue TextValues;

        [Serializable]
        public class MaterialValue
        {
            /// <summary>
            /// Defines the path of the value that is to be applied to the material of the maskable graphic.
            /// </summary>
            public string ValuePath;
            /// <summary>
            /// Defines the maskable graphic's material property adjusted by the value.
            /// </summary>
            public string MaterialPropertyName;
        }
        [Space(10)]
        public MaterialValue[] MaterialValues;

    }
    [Space(15)]
    public Component[] Components;

    
    private List<string> _events = new List<string>();

    // Defines the canvas scaler component of the game object.
    private CanvasScaler _canvasScaler;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void LateUpdate()
    {
        UpdateFeed();
        UpdateComponents();
        UpdateWaypoints();
        UpdatePointers();
        UpdateEmojis();
        UpdateRect();

        ClearEvents();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        _canvasScaler = this.gameObject.GetComponent<CanvasScaler>();
        this.transform.SetParent(Globals.Instance.Containers.UI);
        SetComponentDefaults();
    }



    private void UpdateRect()
    {
        // Define current screen size
        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
        if (_activeHudSize != currentScreenSize)
        {
            // If the screen size is not equal to that of the active screen size
            // set the active screen size to the current and re-appropriate rect
            _activeHudSize = currentScreenSize;
            AppropriateRect();
        }
    }
    public void AppropriateRect()
    {
        if (LocalPlayer == null)
            return;
        // Determine scale factors
        ScaleFactor = Screen.width / ReferenceWidth;
        float scaleFactorPercentage = 1 / _canvasScaler.scaleFactor;

        // Define rect position by the cameras rect
        Vector2 position;
        position.x = (Screen.width / 2) * LocalPlayer.CameraController.Camera.rect.width;
        position.x += Screen.width * LocalPlayer.CameraController.Camera.rect.x;
        position.y = (Screen.height / 2) * LocalPlayer.CameraController.Camera.rect.height;
        position.y += Screen.height * LocalPlayer.CameraController.Camera.rect.y;
        // Set position
        HudContainer.anchoredPosition = position * scaleFactorPercentage;

        Vector2 size = new Vector2(Screen.width * LocalPlayer.CameraController.Camera.rect.width, Screen.height * LocalPlayer.CameraController.Camera.rect.height);
        // Set size
        HudContainer.sizeDelta = size * scaleFactorPercentage;
    }


    private void UpdateFeed()
    {
        FeedText.text = HUDManager.Instance.Feed.String;
    }


    // Events
    public void AddEvent(string name)
    {
        _events.Add(name);
    }
    private void ClearEvents()
    {
        _events.Clear();
    }
    public void RemoveEvent(string name)
    {
        _events.RemoveAll(x => x == name);
    }


    // Pointers
    private void UpdatePointers()
    {
        for (int pointerIndex = 0; pointerIndex < _pointers.Count; pointerIndex++)
        {
            Pointer hudDamageIndicator = _pointers[pointerIndex];
            if (hudDamageIndicator.RectTransform == null)
            {
                _pointers.RemoveAt(pointerIndex);
                pointerIndex--;
                continue;
            }

            float x = LocalPlayer.CameraController.transform.position.x - hudDamageIndicator.Position.x;
            float z = LocalPlayer.CameraController.transform.position.z - hudDamageIndicator.Position.z;
            float angle = Mathf.Atan2(x, z) * Mathf.Rad2Deg - LocalPlayer.CameraController.transform.eulerAngles.y;
            if (angle < 180)
                angle *= -1;

            hudDamageIndicator.RectTransform.eulerAngles = new Vector3(0, 0, angle);
        }
    }
    public void AddPointer(Pointer.Type type, Vector3 targetPosition)
    {
        GameObject pointerPrefab = DamagePointerPrefab;
        // Add new damage indicator
        switch (type)
        {
            case Pointer.Type.Damage:
                break;
            case Pointer.Type.Alert:
                pointerPrefab = AlertPointerPrefab;
                break;

        }

        GameObject newPointer = Instantiate(pointerPrefab, PointerContainer);
        RectTransform rectTransform = newPointer.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        rectTransform.anchoredPosition = Vector2.zero;
        _pointers.Add(new Pointer(targetPosition, rectTransform));
    }
    public void RemoveAllPointers()
    {
        foreach (Pointer damagePointer in _pointers)
        {
            if(damagePointer.RectTransform != null)
                Destroy(damagePointer.RectTransform.gameObject);
        }
        _pointers.Clear();
    }


    // Waypoints
    private void UpdateWaypoints()
    {
        // Reposition and sort waypoints
        Dictionary<int, float> distances = new Dictionary<int, float>();
        for (int waypointIndex = 0; waypointIndex < _waypoints.Count; waypointIndex++)
        {
            Waypoint hudWaypoint = _waypoints[waypointIndex];

            // Destroy waypoint if either dependent transform is missing
            if (hudWaypoint.Transform == null || hudWaypoint.RectTransform == null)
            {
                if(hudWaypoint.RectTransform != null)
                    Destroy(hudWaypoint.RectTransform.gameObject);

                _waypoints.RemoveAt(waypointIndex);
                waypointIndex--;
                continue;
            }

            // Add to object distance collection for sort order update
            distances.Add(waypointIndex, Vector3.Distance(LocalPlayer.CameraController.transform.position, hudWaypoint.Transform.position + hudWaypoint.WorldOffset));

            // Reposition waypoint to new world position, set visibility
            hudWaypoint.RectTransform.position = LocalPlayer.CameraController.Camera.WorldToScreenPoint(hudWaypoint.Transform.position + hudWaypoint.WorldOffset);
            hudWaypoint.RectTransform.gameObject.SetActive(hudWaypoint.Active && 0 <= hudWaypoint.RectTransform.position.z && hudWaypoint.Transform.gameObject.activeSelf);
        }

        // Update sort order
        foreach (KeyValuePair<int, float> distance in distances.OrderBy(i => i.Value))
            _waypoints[distance.Key].RectTransform.SetAsFirstSibling();
    }
    public bool AddWaypoint(Waypoint.WaypointType type, Transform transform, Vector3 worldOffset, string text = "", Color color = new Color())
    {
        Waypoint existingWaypoint = _waypoints.Find(p => p.Transform == transform);
        if (existingWaypoint != null)
            return false;

        GameObject newWaypoint = null;
        switch (type)
        {
            case Waypoint.WaypointType.TeamMate:
                newWaypoint = Instantiate(HUDManager.Instance.Waypoints.TeamMatePrefab, WaypointsContainer);
                break;
            case Waypoint.WaypointType.ObjectiveCapture:
                newWaypoint = Instantiate(HUDManager.Instance.Waypoints.CapturePrefab, WaypointsContainer);
                break;
            case Waypoint.WaypointType.ObjectiveKOTH:
                newWaypoint = Instantiate(HUDManager.Instance.Waypoints.KOTHPrefab, WaypointsContainer);
                break;
            case Waypoint.WaypointType.ObjectiveArm:
                newWaypoint = Instantiate(HUDManager.Instance.Waypoints.ArmPrefab, WaypointsContainer);
                break;
            case Waypoint.WaypointType.ObjectiveBomb:
                newWaypoint = Instantiate(HUDManager.Instance.Waypoints.BombPrefab, WaypointsContainer);
                break;
            case Waypoint.WaypointType.AlertAttached:
                newWaypoint = Instantiate(HUDManager.Instance.Waypoints.ControlAlertPrefab, WaypointsContainer);
                break;
            case Waypoint.WaypointType.Enemy:
                newWaypoint = Instantiate(HUDManager.Instance.Waypoints.EnemyPrefab, WaypointsContainer);
                break;
            case Waypoint.WaypointType.Beacon:
                newWaypoint = Instantiate(HUDManager.Instance.Waypoints.BeaconPrefab, WaypointsContainer);
                break;
        }
        HeadsUpDisplayWaypoint hudWaypoint = newWaypoint.GetComponent<HeadsUpDisplayWaypoint>();

        if (text != string.Empty)
            hudWaypoint.Text = text;
        if (color != new Color())
            hudWaypoint.Color = color;

        RectTransform rectTransform = newWaypoint.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        _waypoints.Add(new Waypoint(type, transform, rectTransform, worldOffset));

        // Return if the waypoint is on screen
        rectTransform.position = LocalPlayer.CameraController.Camera.WorldToScreenPoint(transform.position + worldOffset);
        return 0 <= rectTransform.position.z;
    }
    public void RefreshWaypoint(Transform transform, string text, Color color, float progress = 0)
    {
        Waypoint existingWaypoint = _waypoints.Find(p => p.Transform == transform);
        if (existingWaypoint != null)
        {
            HeadsUpDisplayWaypoint hudWaypoint = existingWaypoint.RectTransform.GetComponent<HeadsUpDisplayWaypoint>();
            hudWaypoint.Progress = progress;
            hudWaypoint.Color = color;
            hudWaypoint.Text = text;
        }
    }
    public void RemoveWaypoint(Transform transform)
    {
        foreach (Waypoint waypoint in _waypoints)
            if (waypoint.Transform == transform)
                waypoint.Transform = null;
    }
    public void ClearWaypointsOfType(Waypoint.WaypointType type)
    {
        foreach (Waypoint waypoint in _waypoints)
            if (waypoint.Type == type)
                Destroy(waypoint.RectTransform.gameObject);
    }


    // Emojis
    private void UpdateEmojis()
    {
        for (int i = 0; i < _emojis.Count; i++)
        {
            Emoji emoji = _emojis[i];

            // Clear active if completed
            if (emoji.Active)
            {
                if (emoji.RectTransform == null)
                    _emojis.Remove(emoji);
                return;
            }

            GameObject newEmoji = Instantiate(EmojiPrefabs[emoji.Type].Emoji, EmojisContainer);
            HeadsUpDisplayEmoji headsUpDisplayEmoji = newEmoji.GetComponent<HeadsUpDisplayEmoji>();
            headsUpDisplayEmoji.Text = emoji.Text;
            emoji.RectTransform = newEmoji.GetComponent<RectTransform>();
            emoji.RectTransform.anchoredPosition = Vector3.zero;
            emoji.Active = true;
            return;
        }
    }

    public void AddEmoji(byte type, string fromName)
    {
        _emojis.Add(new Emoji(type, fromName));
    }


    // Components
    private void UpdateComponents()
    {
        if (LocalPlayer == null)
            return;
        
        foreach (Component component in Components)
        {
            SetComponentCases(component);
            SetComponentEvents(component);
            SetComponentTextValue(component);
            SetComponentMaterialValue(component);
        }
    }
    private void SetComponentDefaults()
    {
        foreach (Component component in Components)
        {
            if (component.MaskableGraphic == null)
                continue;

            component.Text = component.MaskableGraphic.gameObject.GetComponent<Text>();
            
            if (component.MaskableGraphic != null && component.InstantiateMaterial)
                component.MaskableGraphic.material = new Material(component.MaskableGraphic.material);
        }
    }
    private void SetComponentCases(Component component)
    {
        if (component.ValueCaseSets.Length == 0)
            return;

        bool visible = true;
        foreach (Component.ValueCaseSet valueCases in component.ValueCaseSets)
        {
            foreach (Component.ValueCaseSet.ValueCase valueCase in valueCases.Cases)
            {
                object value = GetHudValue(valueCases.ValuePath);
                string currentCase = value != null ? value.ToString() : "Null";

                if (currentCase == valueCase.Case)
                {
                    // If the case matches
                    if (valueCase.Visibility.Use)
                        // Add to current visibility
                        visible = visible && valueCase.Visibility.Visible;
                    else
                        visible = component.MaskableGraphic.gameObject.activeSelf;

                    if (valueCase.Color.Use)
                    {
                        // Set both the components maskable graphic color and material color
                        component.MaskableGraphic.color = valueCase.Color.Color;
                        if (valueCase.Color.MaterialColorPropertyName != string.Empty)
                            component.MaskableGraphic.material.SetColor(valueCase.Color.MaterialColorPropertyName, valueCase.Color.Color);
                    }
                    if (valueCase.Animation.Use && !valueCase.Animation.ActiveCaseState)
                    {
                        // If the case isn't currently active, define active case state, and and play animation
                        valueCase.Animation.ActiveCaseState = true;
                        valueCase.Animation.UIAnimationSet.Play(valueCase.Animation.AnimationName, valueCase.Animation.AllowReplay);
                    }
                }
                else
                {
                    // If the case doesn't match
                    if (valueCase.Visibility.Use)
                        // Add to current visibility
                        visible &= !valueCase.Visibility.Visible;
                    else
                        visible = component.MaskableGraphic.gameObject.activeSelf;

                    if (valueCase.Animation.Use)
                        // Define active case state
                        valueCase.Animation.ActiveCaseState = false;
                }
            }
        }
        component.MaskableGraphic.gameObject.SetActive(visible);
    }
    private void SetComponentEvents(Component component)
    {
        foreach (Component.HudEvent hudEvent in component.Events)
        {
            bool eventTriggered = _events.Contains(hudEvent.Event);
            if (eventTriggered)
            {
                // If the case matches

                // Set visiblity
                if (hudEvent.Visibility.Use)
                    component.MaskableGraphic.gameObject.SetActive(hudEvent.Visibility.Visible);

                if (hudEvent.Color.Use)
                {
                    // Set both the components maskable graphic color and material color
                    component.MaskableGraphic.color = hudEvent.Color.Color;
                    if (hudEvent.Color.MaterialColorPropertyName != string.Empty)
                        component.MaskableGraphic.material.SetColor(hudEvent.Color.MaterialColorPropertyName, hudEvent.Color.Color);
                }

                // Play animation
                if (hudEvent.Animation.Use)
                    hudEvent.Animation.UIAnimationSet.Play(hudEvent.Animation.AnimationName, hudEvent.Animation.AllowReplay);
            }
        }
    }
    private void SetComponentTextValue(Component component)
    {
        if (component.Text == null || component.TextValues.TextValuePaths.Length == 0)
            return;

        string[] strings = new string[component.TextValues.TextValuePaths.Length];
        bool allEmpty = true;
        for (int pathIndex = 0; pathIndex < component.TextValues.TextValuePaths.Length; pathIndex++)
        {
            // Define value
            System.Object value = GetHudValue(component.TextValues.TextValuePaths[pathIndex]);

            if (value == null)
            {
                // If the the value was null set the string to empty and continue
                strings[pathIndex] = string.Empty;
                continue;
            }

            // If the value is to be rounded, round and convert to string; Otherwise just convert to string
            string valueString = string.Empty;
            if (component.TextValues.TextValueMod != Component.TextValue.TextModType.None)
                valueString = SetTextModification(component.TextValues.TextValueMod, value);
            else
                valueString = value.ToString();


            // If all empty is true and the string isn't empty redefine all empty
            if (allEmpty && valueString != string.Empty)
                allEmpty = false;

            // Define string at index
            strings[pathIndex] = valueString;
        }

        // Define format
        string result = component.TextValues.TextValueFormat == string.Empty || allEmpty ? strings[0] : string.Format(component.TextValues.TextValueFormat, strings);
        // Omit string from value string
        if (component.TextValues.TextValueOmmision != string.Empty)
            result = result.Replace(component.TextValues.TextValueOmmision, string.Empty);

        // Define case
        result = SetTextCase(component.TextValues.TextValueCase, result);
        // Set text
        component.Text.text = result;
    }
    private void SetComponentMaterialValue(Component component)
    {
        foreach (Component.MaterialValue materialValue in component.MaterialValues)
        {
            // Define value
            System.Object value = GetHudValue(materialValue.ValuePath);
            // If the value isn't null, set the materials property
            if (value != null)
                component.MaskableGraphic.material.SetFloat(materialValue.MaterialPropertyName, (float)value);
        }
    }


    private object GetHudValue(string valuePath)
    {
        // If the value was not retrieved using the player as the root object, attempt using the game manager

        if (valuePath != string.Empty)
        {
            // Define the root of the method to be invoked
            string[] directories = valuePath.Split('/');

            string path = valuePath.Replace(directories[0] + "/", string.Empty);

            bool local = false;
            object root = null;

            if (directories[0] == "profile")
                root = LocalPlayer.Profile;
            else if (directories[0] == "game_manager")
                root = GameManager.Instance;
            else
            {
                local = true;
                root = this;
            }

            if (root != null)
                return ObjectReferencer.GetValue(root, local ? valuePath : path);
        }
        return null;
    }


    private string SetTextModification(Component.TextValue.TextModType textModType, System.Object value)
    {
        float floatValue = 0;

        Type type = value.GetType();

        if (type == typeof(float))
            floatValue = (float)value;
        else if (type == typeof(int))
            floatValue = (float)(int)value;
        else if (type == typeof(short))
            floatValue = (float)(short)value;


        switch (textModType)
        {
            case Component.TextValue.TextModType.Ceiling:
                return Mathf.CeilToInt(floatValue).ToString();
            case Component.TextValue.TextModType.Floor:
                return Mathf.FloorToInt(floatValue).ToString();
            case Component.TextValue.TextModType.TimeSpanMSS:
                string time = string.Empty;
                if (floatValue != 0)
                {
                    TimeSpan timeSpan = TimeSpan.FromSeconds(floatValue);
                    return timeSpan.Minutes + ":" + timeSpan.Seconds.ToString("D2");
                }
                return time;
            case Component.TextValue.TextModType.AddPlusToPositive:
                if (floatValue > 0)
                    return "+" + floatValue.ToString();
                return floatValue.ToString();
        }
        return string.Empty;
    }
    private string SetTextCase(Component.TextValue.TextCaseType textCaseType, string text)
    {
        string result = text;
        switch (textCaseType)
        {
            case Component.TextValue.TextCaseType.None:
                break;
            case Component.TextValue.TextCaseType.ToLower:
                result = result.ToLower();
                break;
            case Component.TextValue.TextCaseType.ToUpper:
                result = result.ToUpper();
                break;
        }
        return result;
    }
    #endregion
}