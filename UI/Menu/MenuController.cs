using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    #region Values
    public static MenuController Instance;

    public float MovementStunDelay = 0.243f;
    private float _nextMoveTime;

    public bool DisableOnGameEnd;
    public enum InputButton
    {
        B,
        A,
        X,
        Y
    }
    public MenuControlGroup DefaultGroup;
    public MenuControlGroup StartButtonGroup;

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
        UpdateInput();
    }
    #endregion

    #region Functions
    public void SetActiveGroup(MenuControlGroup group, bool closeActiveGroup = false)
    {
        if (closeActiveGroup && _activeGroup != null)
            _activeGroup.Close(true);

        _activeGroup = group;
    }


    private void UpdateInput()
    {
        // If input is disabled upon game end and the game has ended, abort
        if (DisableOnGameEnd && !GameManager.Instance.Game.Playing)
            return;
        // If there is an active group and that groups input time has yet to be met, abort
        if (_activeGroup != null && _activeGroup.AllowInputTime > Time.time)
            return;

        
        bool showingStartMenu = false;

        // Update each controller
        for (int i = 0; i < 4; i++)
        {
            GameManager.GameAspects.Profile profile = GameManager.Instance.Game.Profiles.Find(p => p.ControllerId == i && p.Local);

            bool ignoreB = false;
            bool ignoreA = false;
            bool ignoreX = false;
            bool ignoreY = false;
            if (_activeGroup != null)
            {
                // Non bias input (bias is handled in the button action's settings)
                if (GamePadManager.GetButtonDown(i, GamePadManager.Button.B))
                    ignoreB = _activeGroup.InvokeAction(InputButton.B, i, profile);
                if (GamePadManager.GetButtonDown(i, GamePadManager.Button.A))
                    ignoreA = _activeGroup.InvokeAction(InputButton.A, i, profile);
                if (GamePadManager.GetButtonDown(i, GamePadManager.Button.X))
                    ignoreX = _activeGroup.InvokeAction(InputButton.X, i, profile);
                if (GamePadManager.GetButtonDown(i, GamePadManager.Button.Y))
                    ignoreY = _activeGroup.InvokeAction(InputButton.Y, i, profile);
            }

            if (profile == null)
                continue;

            // Registered bias input

            if (GamePadManager.GetButtonDown(profile.ControllerId, GamePadManager.Button.Start))
            {
                // Show start menu
                if (_activeGroup == null || _activeGroup.AllowStartMenu)
                {
                    _activeGroup = StartButtonGroup;
                    StartButtonGroup.Show(profile);
                }
            }

            if (GameManager.Instance.Game.Playing)
            {
                if (GamePadManager.GetButton(profile.ControllerId, GamePadManager.Button.Back) && !showingStartMenu)
                {
                    // Show start menu
                    MultiplayerManager.Instance.Scoreboard.SetActive(true);
                    showingStartMenu = true;
                }
            }

            if (_activeGroup != null)
            {
                if (_activeGroup.DisabledForClient && NetworkSessionManager.IsClient)
                    continue;

                // If there is an active group, and input is allowed to move, move
                if (_nextMoveTime < Time.time)
                {
                    bool moved = false;
                    Vector2 leftStick = GamePadManager.GetDirection(profile.ControllerId, GamePadManager.Direction.LeftStick);

                    if (GamePadManager.GetButton(profile.ControllerId, GamePadManager.Button.DpadUp) || leftStick.y > 0.5f)
                        moved = _activeGroup.ScrollRow(false, profile);
                    if (GamePadManager.GetButton(profile.ControllerId, GamePadManager.Button.DpadDown) || leftStick.y < -0.5f)
                        moved = _activeGroup.ScrollRow(true, profile);

                    if (GamePadManager.GetButton(profile.ControllerId, GamePadManager.Button.DpadRight) || leftStick.x > 0.5f)
                        moved = _activeGroup.ScrollColumn(true, profile);
                    if (GamePadManager.GetButton(profile.ControllerId, GamePadManager.Button.DpadLeft) || leftStick.x < -0.5f)
                        moved = _activeGroup.ScrollColumn(false, profile);

                    if (moved)
                        _nextMoveTime = Time.time + MovementStunDelay;

                    if (GamePadManager.GetTriggerRaw(profile.ControllerId, GamePadManager.Trigger.Right) > 0.1f)
                        moved = _activeGroup.ScrollToEnd(true, profile);
                    if (GamePadManager.GetTriggerRaw(profile.ControllerId, GamePadManager.Trigger.Left) > 0.1f)
                        moved = _activeGroup.ScrollToEnd(false, profile);

                }

                // Setting Bumper input
                if (GamePadManager.GetButtonDown(profile.ControllerId, GamePadManager.Button.LB))
                    _activeGroup.InvokeSetting(false, profile);
                if (GamePadManager.GetButtonDown(profile.ControllerId, GamePadManager.Button.RB))
                    _activeGroup.InvokeSetting(true, profile);
                // Button A input
                if (!ignoreA && GamePadManager.GetButtonDown(profile.ControllerId, GamePadManager.Button.A))
                    _activeGroup.InvokeButton(profile);
            }
        }


        if ((!showingStartMenu || _activeGroup != null) && MultiplayerManager.Instance != null)
            MultiplayerManager.Instance.Scoreboard.SetActive(false);
    }
    
    #endregion
}
