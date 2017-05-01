using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class XboxInputManager : MonoBehaviour 
{
    #region Values
    // Defines the instance of this class.
    private static XboxInputManager _instance;

    /// <summary>
    /// Defines the quantity of connected controllers.
    /// </summary>
    public static int ConnectedControllerCount;
    /// <summary>
    /// Defines a section of dead space for each trigger to return false if its value is smaller.
    /// </summary>
    private const float TRIGGER_DEAD_SPACE = 0.243f;

    public enum Trigger
    {
        Left,
        Right
    }
    public enum Button
    {
        Guide,
        Start,
        Back,
        A,
        B,
        X,
        Y,
        LB,
        RB,
        LeftStick,
        RightStick,
        DpadUp,
        DpadDown,
        DpadLeft,
        DpadRight
    }
    public enum Direction
    {
        LeftStick,
        RightStick,
        Dpad
    }
    
    private class GamePadStates
    {
        public GamePadState PreviousState = new GamePadState();
        public GamePadState CurrentState = new GamePadState();
    }
    private GamePadStates[] _gamePadStates;

    public class Vibration
    {
        /// <summary>
        /// Defines the id of the controller that is supposed to recieve vibration.
        /// </summary>
        public int ControllerId;
        /// <summary>
        /// Defines the intensity of the vibration.
        /// </summary>
        public float Intensity;
        /// <summary>
        /// Defines the duration of the vibration.
        /// </summary>
        public float StartTime;
        /// <summary>
        /// Defines the remaining duration of the vibration.
        /// </summary>
        public float KillTime;

        public Vibration(int controllerID, float intensity, float duration)
        {
            ControllerId = controllerID;
            Intensity = intensity;
            StartTime = Time.time;
            KillTime = Time.time + duration;
        }
    }
    public List<Vibration> Vibrations = new List<Vibration>();
    private float[] _controllerVibrations; 
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        UpdateVibrations();
        UpdateStates();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        // Create instance, or destroy self if duplicate
        if (_instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        SetDefaults();
    }

    private void SetDefaults()
    {
        _gamePadStates = new GamePadStates[] { new GamePadStates(), new GamePadStates(), new GamePadStates(), new GamePadStates() };
        _controllerVibrations = new float[] { 0, 0, 0, 0 };
    }

    private void UpdateStates()
    {
        ConnectedControllerCount = 0;
        for (int i = 0; i < 4; i++)
        {
            _gamePadStates[i].PreviousState = _gamePadStates[i].CurrentState;
            _gamePadStates[i].CurrentState = GamePad.GetState((PlayerIndex)i);

            if (_gamePadStates[i].CurrentState.IsConnected)
                ConnectedControllerCount += 1;

            float vibrationIntensity = Mathf.Min(_controllerVibrations[i], 1);
            GamePad.SetVibration((PlayerIndex)i, vibrationIntensity, vibrationIntensity);
        }
    }

    public static bool IsConnected(int gamePad)
    {
        return _instance._gamePadStates[gamePad].CurrentState.IsConnected;
    }

    public static bool GetTrigger(int gamePad, Trigger trigger)
    {
        if (trigger == Trigger.Right)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Right > TRIGGER_DEAD_SPACE;
        else if (trigger == Trigger.Left)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Left > TRIGGER_DEAD_SPACE;

        return false;
    }
    public static bool GetTriggerUp(int gamePad, Trigger trigger)
    {
        if (trigger == Trigger.Right)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Right < TRIGGER_DEAD_SPACE && _instance._gamePadStates[gamePad].PreviousState.Triggers.Right > TRIGGER_DEAD_SPACE;
        else if (trigger == Trigger.Left)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Left < TRIGGER_DEAD_SPACE && _instance._gamePadStates[gamePad].PreviousState.Triggers.Left > TRIGGER_DEAD_SPACE;

        return false;
    }
    public static bool GetTriggerDown(int gamePad, Trigger trigger)
    {
        if (trigger == Trigger.Right)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Right > TRIGGER_DEAD_SPACE && _instance._gamePadStates[gamePad].PreviousState.Triggers.Right < TRIGGER_DEAD_SPACE;
        else if (trigger == Trigger.Left)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Left > TRIGGER_DEAD_SPACE && _instance._gamePadStates[gamePad].PreviousState.Triggers.Left < TRIGGER_DEAD_SPACE;

        return false;
    }
    public static float GetTriggerRaw(int gamePad, Trigger trigger)
    {
        if (trigger == Trigger.Right)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Right;
        else if (trigger == Trigger.Left)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Left;

        return 0;
    }

    public static bool GetButton(int gamePad, Button button)
    {
        return GetButtonState(_instance._gamePadStates[gamePad].CurrentState, gamePad, button);
    }
    public static bool GetButtonUp(int gamePad, Button button)
    {
        bool currentState = GetButtonState(_instance._gamePadStates[gamePad].CurrentState, gamePad, button);
        bool previousState = GetButtonState(_instance._gamePadStates[gamePad].PreviousState, gamePad, button);
        return previousState && !currentState;
    }
    public static bool GetButtonDown(int gamePad, Button button)
    {
        bool currentState = GetButtonState(_instance._gamePadStates[gamePad].CurrentState, gamePad, button);
        bool previousState = GetButtonState(_instance._gamePadStates[gamePad].PreviousState, gamePad, button);
        return !previousState && currentState;
    }

    public static bool GetButtonState(GamePadState gamePadState, int gamePad, Button button)
    {
        ButtonState currentState = ButtonState.Released;
        if (button == Button.Guide)
            currentState = gamePadState.Buttons.Guide;
        else if (button == Button.Start)
            currentState = gamePadState.Buttons.Start;
        else if (button == Button.Back)
            currentState = gamePadState.Buttons.Back;
        else if (button == Button.A)
            currentState = gamePadState.Buttons.A;
        else if (button == Button.B)
            currentState = gamePadState.Buttons.B;
        else if (button == Button.X)
            currentState = gamePadState.Buttons.X;
        else if (button == Button.Y)
            currentState = gamePadState.Buttons.Y;
        else if (button == Button.LB)
            currentState = gamePadState.Buttons.LeftShoulder;
        else if (button == Button.RB)
            currentState = gamePadState.Buttons.RightShoulder;
        else if (button == Button.LeftStick)
            currentState = gamePadState.Buttons.LeftStick;
        else if (button == Button.RightStick)
            currentState = gamePadState.Buttons.RightStick;
        else if (button == Button.DpadUp)
            currentState = gamePadState.DPad.Up;
        else if (button == Button.DpadDown)
            currentState = gamePadState.DPad.Down;
        else if (button == Button.DpadLeft)
            currentState = gamePadState.DPad.Left;
        else if (button == Button.DpadRight)
            currentState = gamePadState.DPad.Right;

        return currentState == ButtonState.Pressed;
    }

    public static Vector2 GetDirection(int gamePad, Direction direction)
    {
        if (direction == Direction.LeftStick)
            return new Vector2(_instance._gamePadStates[gamePad].CurrentState.ThumbSticks.Left.X, _instance._gamePadStates[gamePad].CurrentState.ThumbSticks.Left.Y);
        else if (direction == Direction.RightStick)
            return new Vector2(_instance._gamePadStates[gamePad].CurrentState.ThumbSticks.Right.X, _instance._gamePadStates[gamePad].CurrentState.ThumbSticks.Right.Y);
        else if (direction == Direction.Dpad)
        {
            float x = 0;
            if (_instance._gamePadStates[gamePad].CurrentState.DPad.Right == ButtonState.Pressed)
                x = 1;
            if (_instance._gamePadStates[gamePad].CurrentState.DPad.Left == ButtonState.Pressed)
                x = -1;

            float y = 0;
            if (_instance._gamePadStates[gamePad].CurrentState.DPad.Down == ButtonState.Pressed)
                y = 1;
            if (_instance._gamePadStates[gamePad].CurrentState.DPad.Up == ButtonState.Pressed)
                y = -1;
            return new Vector2(x, y);
        }


        return Vector2.zero;
    }


    private void UpdateVibrations()
    {
        _controllerVibrations = new float[] { 0, 0, 0, 0 };
        for (int i = 0; i < Vibrations.Count; i++)
        {
            Vibration vibration = Vibrations[i];
            float duration = vibration.KillTime - vibration.StartTime;
            float timeRemaining = vibration.KillTime - Time.time;

            _controllerVibrations[vibration.ControllerId] += vibration.Intensity * (timeRemaining / duration);

            if (Time.time >= vibration.KillTime)
            {
                i--;
                Vibrations.Remove(vibration);
            }
        }
    }

    public static void AddVibration(Vibration vibration)
    {
        _instance.Vibrations.Add(vibration);
    }
    public static void ClearAllVibrations()
    {
        _instance.Vibrations.Clear();
    }
    public static void ClearControllerVibrations(int controllerId)
    {
        _instance.Vibrations.RemoveAll(v => v.ControllerId == controllerId);
    }
    #endregion
}
