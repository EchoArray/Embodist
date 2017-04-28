using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class GamePadManager : MonoBehaviour 
{
    #region Values
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
    public static int ConnectedControllerCount;
    private static GamePadManager _instance;

    private class GamePadStates
    {
        public GamePadState PreviousState = new GamePadState();
        public GamePadState CurrentState = new GamePadState();
    }
    private GamePadStates[] _gamePadStates;

    public class Vibration
    {
        public int ControllerId;
        public float Intensity;
        public float Duration;
        public float LifespanRemaining;
        public Vibration(int controllerID, float intensity, float duration)
        {
            ControllerId = controllerID;
            Intensity = intensity;
            Duration = duration;
            LifespanRemaining = duration;
        }
    }
    public List<Vibration> Vibrations = new List<Vibration>();
    private float[] _controllerVibrations; 
    #endregion

    #region Unity Functions
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        _gamePadStates = new GamePadStates[] { new GamePadStates(), new GamePadStates(), new GamePadStates(), new GamePadStates() };

        _controllerVibrations = new float[] { 0, 0, 0, 0 };
    }

    private void Update()
    {
        UpdateVibrations();
        UpdateStates();
    } 
    #endregion

    #region Functions
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
    private void UpdateVibrations()
    {
        _controllerVibrations = new float[] { 0, 0, 0, 0 };
        for (int i = 0; i < Vibrations.Count; i++)
        {
            Vibration vibration = Vibrations[i];
            _controllerVibrations[vibration.ControllerId] += vibration.Intensity * (vibration.LifespanRemaining / vibration.Duration);

            vibration.LifespanRemaining = Mathf.Max(vibration.LifespanRemaining - Time.deltaTime, 0);
            if (vibration.LifespanRemaining == 0)
                Vibrations.Remove(vibration);
        }
    }
    public static void ClearAllVibrations()
    {
        _instance.Vibrations.Clear();
    }
    public static void ClearControllerVibrations(int controllerId)
    {
        for (int i = 0; i < _instance.Vibrations.Count; i++)
        {
            Vibration vibration = _instance.Vibrations[i];
            if (vibration.ControllerId == controllerId)
            {
                _instance.Vibrations.Remove(vibration);
                i--;
            }
        }
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

    private const float TriggerSensitivity = 0.243f;
    public static float GetTriggerRaw(int gamePad, Trigger trigger)
    {
        if (trigger == Trigger.Right)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Right;
        else if (trigger == Trigger.Left)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Left;

        return 0;
    }
    public static bool GetTriggerDown(int gamePad, Trigger trigger)
    {
        if (trigger == Trigger.Right)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Right > TriggerSensitivity && _instance._gamePadStates[gamePad].PreviousState.Triggers.Right < TriggerSensitivity;
        else if (trigger == Trigger.Left)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Left > TriggerSensitivity && _instance._gamePadStates[gamePad].PreviousState.Triggers.Left < TriggerSensitivity;

        return false;
    }
    public static bool GetTriggerUp(int gamePad, Trigger trigger)
    {
        if (trigger == Trigger.Right)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Right < TriggerSensitivity && _instance._gamePadStates[gamePad].PreviousState.Triggers.Right > TriggerSensitivity;
        else if (trigger == Trigger.Left)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Left < TriggerSensitivity && _instance._gamePadStates[gamePad].PreviousState.Triggers.Left > TriggerSensitivity;

        return false;
    }
    public static bool GetTrigger(int gamePad, Trigger trigger)
    {
        if (trigger == Trigger.Right)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Right > TriggerSensitivity;
        else if (trigger == Trigger.Left)
            return _instance._gamePadStates[gamePad].CurrentState.Triggers.Left > TriggerSensitivity;

        return false;
    }

    public static bool GetButtonDown(int gamePad, Button button)
    {
        if (button == Button.Guide)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.Guide;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.Guide;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }
        if (button == Button.Start)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.Start;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.Start;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }
        if (button == Button.Back)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.Back;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.Back;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }
        if (button == Button.A)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.A;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.A;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }
        else if (button == Button.B)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.B;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.B;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }
        else if (button == Button.X)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.X;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.X;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }
        else if (button == Button.Y)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.Y;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.Y;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }
        else if (button == Button.LB)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.LeftShoulder;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.LeftShoulder;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }
        else if (button == Button.RB)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.RightShoulder;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.RightShoulder;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }
        else if (button == Button.LeftStick)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.LeftStick;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.LeftStick;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }
        else if (button == Button.RightStick)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.RightStick;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.RightStick;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }
        else if (button == Button.DpadUp)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.DPad.Up;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.DPad.Up;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }
        else if (button == Button.DpadDown)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.DPad.Down;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.DPad.Down;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }
        else if (button == Button.DpadLeft)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.DPad.Left;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.DPad.Left;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }
        else if (button == Button.DpadRight)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.DPad.Right;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.DPad.Right;
            bool down = currentState == ButtonState.Pressed && previousState == ButtonState.Released;
            return down;
        }

        return false;
    }
    public static bool GetButtonUp(int gamePad, Button button)
    {
        if (button == Button.Guide)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.Guide;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.Guide;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        if (button == Button.Start)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.Start;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.Start;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        if (button == Button.Back)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.Back;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.Back;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        if (button == Button.A)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.A;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.A;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        else if (button == Button.B)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.B;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.B;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        else if (button == Button.X)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.X;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.X;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        else if (button == Button.Y)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.Y;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.Y;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        else if (button == Button.LB)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.LeftShoulder;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.LeftShoulder;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        else if (button == Button.RB)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.RightShoulder;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.RightShoulder;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        else if (button == Button.LeftStick)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.LeftStick;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.LeftStick;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        else if (button == Button.RightStick)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.RightStick;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.Buttons.RightStick;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        else if (button == Button.DpadUp)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.DPad.Up;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.DPad.Up;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        else if (button == Button.DpadDown)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.DPad.Down;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.DPad.Down;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        else if (button == Button.DpadLeft)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.DPad.Left;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.DPad.Left;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        else if (button == Button.DpadRight)
        {
            ButtonState currentState = _instance._gamePadStates[gamePad].CurrentState.DPad.Right;
            ButtonState previousState = _instance._gamePadStates[gamePad].PreviousState.DPad.Right;
            bool down = previousState == ButtonState.Pressed && currentState == ButtonState.Released;
            return down;
        }
        return false;
    }
    public static bool GetButton(int gamePad, Button button)
    {
        ButtonState currentState = ButtonState.Released;
        if (button == Button.Guide)
            currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.Guide;
        if (button == Button.Start)
            currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.Start;
        if (button == Button.Back)
            currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.Back;
        if (button == Button.A)
            currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.A;
        else if (button == Button.B)
            currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.B;
        else if (button == Button.X)
            currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.X;
        else if (button == Button.Y)
            currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.Y;
        else if (button == Button.LB)
            currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.LeftShoulder;
        else if (button == Button.RB)
            currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.RightShoulder;
        else if (button == Button.LeftStick)
            currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.LeftStick;
        else if (button == Button.RightStick)
            currentState = _instance._gamePadStates[gamePad].CurrentState.Buttons.RightStick;
        else if (button == Button.DpadUp)
            currentState = _instance._gamePadStates[gamePad].CurrentState.DPad.Up;
        else if (button == Button.DpadDown)
            currentState = _instance._gamePadStates[gamePad].CurrentState.DPad.Down;
        else if (button == Button.DpadLeft)
            currentState = _instance._gamePadStates[gamePad].CurrentState.DPad.Left;
        else if (button == Button.DpadRight)
            currentState = _instance._gamePadStates[gamePad].CurrentState.DPad.Right;

        return currentState == ButtonState.Pressed;
    }

    public static void AddVibration(Vibration vibration)
    {
        _instance.Vibrations.Add(vibration);
    }

    #endregion
} 
