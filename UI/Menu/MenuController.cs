using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    #region Values
    public static MenuController Instance;

    /// <summary>
    /// Determines a xbox controller button.
    /// </summary>
    public enum InputButton
    {
        B,
        A,
        X,
        Y
    }

    /// <summary>
    /// Defines the delay between movement actions.
    /// </summary>
    public float MovementStunDelay = 0.243f;
    // Defines the next time that a movement is allowed.
    private float _nextMoveTime;

    /// <summary>
    /// Deteremines if the menu controller will be disabled upon game end.
    /// </summary>
    public bool DisableOnGameEnd;
    /// <summary>
    /// Defines the default group of the menu controller.
    /// </summary>
    public MenuControlGroup DefaultGroup;
    /// <summary>
    /// Defines the menu group show upon pressing the start button on the xbox controller.
    /// </summary>
    public MenuControlGroup StartButtonGroup;
    // Defines the current active group.
    private MenuControlGroup _activeGroup;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Instance = this;

        _activeGroup = DefaultGroup;
        if(_activeGroup != null)
            _activeGroup.Show(null);
    }

    private void Update()
    {
        UpdateXboxControllerInput();
    }
    #endregion

    #region Functions
    public void SetActiveGroup(MenuControlGroup group, bool closeExistingGroup = false)
    {
        if (closeExistingGroup && _activeGroup != null)
            _activeGroup.Close(true);

        _activeGroup = group;
    }
    
    private void UpdateXboxControllerInput()
    {
        // If input is disabled upon game end and the game has ended, abort
        if (DisableOnGameEnd && !GameManager.Instance.Game.Playing)
            return;

        // If there is an active group and that groups input time has yet to be met, abort
        if (_activeGroup != null && _activeGroup.AllowInputTime > Time.time)
            return;
        
        bool showingScoreboard = false;

        // Update each controller
        for (int i = 0; i < 4; i++)
        {
            // Check if the target controller is active
            if (!XboxInputManager.IsConnected(i))
                continue;
            
            // Attempt to get profile for the controller index
            GameManager.GameAspects.Profile profile = GameManager.Instance.Game.Profiles.Find(p => p.ControllerId == i && p.Local);

            // Buttons to ignore for this controller for the duration of the frame after push (if specified in menu group)
            bool ignoreB = false;
            bool ignoreA = false;
            bool ignoreX = false;
            bool ignoreY = false;

            // Non bias input (bias is handled in the button action's settings)
            if (_activeGroup != null)
            {
                if (XboxInputManager.GetButtonDown(i, XboxInputManager.Button.B))
                    ignoreB = _activeGroup.InvokeAction(InputButton.B, i, profile);
                if (XboxInputManager.GetButtonDown(i, XboxInputManager.Button.A))
                    ignoreA = _activeGroup.InvokeAction(InputButton.A, i, profile);
                if (XboxInputManager.GetButtonDown(i, XboxInputManager.Button.X))
                    ignoreX = _activeGroup.InvokeAction(InputButton.X, i, profile);
                if (XboxInputManager.GetButtonDown(i, XboxInputManager.Button.Y))
                    ignoreY = _activeGroup.InvokeAction(InputButton.Y, i, profile);
            }

            // Registered bias input
            if (profile != null)
            {
                // Show start menu
                if (XboxInputManager.GetButtonDown(profile.ControllerId, XboxInputManager.Button.Start))
                {
                    if (_activeGroup == null || _activeGroup.AllowStartMenu)
                    {
                        _activeGroup = StartButtonGroup;
                        StartButtonGroup.Show(profile);
                    }
                }
                // Show scoreboard
                if (GameManager.Instance.Game.Playing && !showingScoreboard && XboxInputManager.GetButton(profile.ControllerId, XboxInputManager.Button.Back))
                {
                    MultiplayerManager.Instance.Scoreboard.SetActive(true);
                    showingScoreboard = true;
                }

                if (_activeGroup != null)
                {
                    if (_activeGroup.DisabledForClient && NetworkSessionManager.IsClient)
                        continue;
                    
                    // Bumper input - Scroll setting
                    if (XboxInputManager.GetButtonDown(profile.ControllerId, XboxInputManager.Button.LB))
                        _activeGroup.InvokeSetting(false, profile);
                    if (XboxInputManager.GetButtonDown(profile.ControllerId, XboxInputManager.Button.RB))
                        _activeGroup.InvokeSetting(true, profile);

                    // A input - Bush button
                    if (!ignoreA && XboxInputManager.GetButtonDown(profile.ControllerId, XboxInputManager.Button.A))
                        _activeGroup.InvokeButton(profile);

                    // Menu scrolling input
                    if (_nextMoveTime < Time.time)
                    {
                        // If there is an active group, and input is allowed to move, move

                        // Used to set movement delay upon movement
                        bool moved = false;
                        
                        // Trigger input
                        if (XboxInputManager.GetTriggerRaw(profile.ControllerId, XboxInputManager.Trigger.Right) > 0.1f)
                            moved = _activeGroup.ScrollToEnd(true, profile);
                        if (XboxInputManager.GetTriggerRaw(profile.ControllerId, XboxInputManager.Trigger.Left) > 0.1f)
                            moved = _activeGroup.ScrollToEnd(false, profile);


                        Vector2 leftStick = XboxInputManager.GetDirection(profile.ControllerId, XboxInputManager.Direction.LeftStick);

                        // Vertical stick / dpad input
                        if (XboxInputManager.GetButton(profile.ControllerId, XboxInputManager.Button.DpadUp) || leftStick.y > 0.5f)
                            moved = _activeGroup.ScrollRow(false, profile);
                        if (XboxInputManager.GetButton(profile.ControllerId, XboxInputManager.Button.DpadDown) || leftStick.y < -0.5f)
                            moved = _activeGroup.ScrollRow(true, profile);

                        // Horizontal stick / dpad input
                        if (XboxInputManager.GetButton(profile.ControllerId, XboxInputManager.Button.DpadRight) || leftStick.x > 0.5f)
                            moved = _activeGroup.ScrollColumn(true, profile);
                        if (XboxInputManager.GetButton(profile.ControllerId, XboxInputManager.Button.DpadLeft) || leftStick.x < -0.5f)
                            moved = _activeGroup.ScrollColumn(false, profile);

                        if (moved)
                            _nextMoveTime = Time.time + MovementStunDelay;
                    }
                }
            }
        }
        
        if (!showingScoreboard && _activeGroup == null && GameManager.Instance.Game.Playing)
            MultiplayerManager.Instance.Scoreboard.SetActive(false);
    }
    #endregion
}
