using UnityEngine;
using System.Collections;
using XInputDotNetPure;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using UnityEngine.Networking;

[ValueReference("player")]
public class LocalPlayer : MonoBehaviour
{
    #region Values
    /// <summary>
    /// Defines the players profile
    /// </summary>
    [HideInInspector]
    [ValueReference("profile")]
    public GameManager.GameAspects.Profile Profile;

    /// <summary>
    /// Defines the players camera controller
    /// </summary>
    [HideInInspector]
    [ValueReference("camera_controller")]
    public CameraController CameraController;

    /// <summary>
    ///  Determines which inanimate object the player is currently controlling
    /// </summary>
    [HideInInspector]
    [ValueReference("inanimate_object")]
    public InanimateObject InanimateObject;

    /// <summary>
    /// Defines the players heads up display
    /// </summary>
    [HideInInspector]
    public HeadsUpDisplay HeadsUpDisplay;

    /// <summary>
    /// Determines if the player is currently dead or alive
    /// </summary>
    [HideInInspector]
    public bool AwaitingRespawn;
    
    private float _beaconCooledDownTime;
    private int _lastKillerGamerId;
    #endregion

    #region Unity Functions
    private void Update()
    {
        UpdateInput();
    }
    #endregion

    #region Functions
    private void UpdateInput()
    {
        bool allowInput = !AwaitingRespawn && GameManager.Instance.Game.AllowInput && GameManager.Instance.Game.Playing;

        Vector2 leftStick = XboxInputManager.GetDirection(Profile.ControllerId, XboxInputManager.Direction.LeftStick);
        if (!allowInput || leftStick.magnitude < 0.05f)
            leftStick = Vector2.zero;

        // Move and orbit camera controller
        Vector2 rightStick = XboxInputManager.GetDirection(Profile.ControllerId, XboxInputManager.Direction.RightStick);
        if (!allowInput || rightStick.magnitude < 0.05f)
            rightStick = Vector2.zero;

        CameraController.Look(new Vector2(rightStick.x, rightStick.y * (Profile.LookInverted ? 1 : -1)));


        if (InanimateObject == null)
        {
            bool boosting = 0.1f < XboxInputManager.GetTriggerRaw(Profile.ControllerId, XboxInputManager.Trigger.Left);
            CameraController.Move(leftStick, boosting);
            if (allowInput)
                CameraController.ProspectSelection(XboxInputManager.GetButtonDown(Profile.ControllerId, XboxInputManager.Button.A));
        }
        else if (allowInput)
        {
            bool leftTrigger = XboxInputManager.GetTrigger(Profile.ControllerId, XboxInputManager.Trigger.Left);
            bool rightTrigger = XboxInputManager.GetTrigger(Profile.ControllerId, XboxInputManager.Trigger.Right);

            if (InanimateObject.Class == InanimateObject.Classification.Throwy)
                rightTrigger = XboxInputManager.GetTriggerDown(Profile.ControllerId, XboxInputManager.Trigger.Right);


            InanimateObject.Aim(leftTrigger);


            // Attack
            if (rightTrigger)
                InanimateObject.Attack();


            // Move and aim attached object
            InanimateObject.Move(leftStick);

            // Set beacon
            if (XboxInputManager.GetButtonDown(Profile.ControllerId, XboxInputManager.Button.RB))
                SetBeacon();
            // Jump
            if (XboxInputManager.GetButtonDown(Profile.ControllerId, XboxInputManager.Button.LB) || XboxInputManager.GetButtonDown(Profile.ControllerId, XboxInputManager.Button.A))
                InanimateObject.Jump();
        }
    }

    public void AddVibration(XboxInputManager.Vibration vibration)
    {
        if (Profile.ControllerVibration)
            XboxInputManager.AddVibration(vibration);
    }

    public void AttachTo(InanimateObject inanimateObject)
    {
        // Set and ready attached object
        InanimateObject = inanimateObject;
        InanimateObject.Select(this);

        if (inanimateObject.Class == InanimateObject.Classification.Bomb)
            HeadsUpDisplay.RemoveWaypoint(inanimateObject.transform);

        if (InanimateObject.Class == InanimateObject.Classification.Smashy)
            HUDManager.Instance.PopulateEnemyWaypoints(HeadsUpDisplay);

        HeadsUpDisplay.AddEvent("player_attached");

        HeadsUpDisplay.AddEvent(InanimateObject.Weapon == null ? "attached_has_no_weapon" : "attached_has_weapon");
    }
    public void ForceAttachTo(InanimateObject inanimateObject)
    {
        CameraController.SetAttributes(true);
        CameraController.UnHighlightProspect();
        AttachTo(inanimateObject);
    }

    public void AttachedDamagedEnemy()
    {
        HeadsUpDisplay.AddEvent("attached_damaged_enemy");
    }
    public void AttachedKilledEnemy(LocalPlayer localPlayer)
    {
        HUDManager.Instance.AddFeedItem(Profile.Name + " GOOPED " + localPlayer.Profile.Name);
        Profile.Kills += 1;
        if(GameManager.Instance.Game.GameType.KillBased)
            GameManager.Instance.AddProfileKill(Profile.GamerId);
    }
    public void AttachedDamaged(Vector3 damagePosition, bool showDirection)
    {
        if (showDirection)
            HeadsUpDisplay.AddPointer(HeadsUpDisplay.Pointer.Type.Damage, damagePosition);
        HeadsUpDisplay.AddEvent("attached_damaged");

        CameraController.CameraEffector.AddEffect(Globals.Instance.InanimateDefaults.TakeDamageCameraEffect);
    }
    public void AttachedDied(bool countDeath, int killerGamerId)
    {
        _lastKillerGamerId = killerGamerId;
        InanimateObject = null;

        HeadsUpDisplay.ClearWaypointsOfType(HeadsUpDisplay.Waypoint.WaypointType.Enemy);
        if (countDeath)
            GameManager.Instance.AddProfileDeath(Profile.GamerId);

        if (killerGamerId != Profile.GamerId)
        {
            GameManager.Instance.AddProfileKill(killerGamerId);
            ShowEmojiMenu();
        }

        if (!AwaitingRespawn)
            StartDelayedRespawn();

        HeadsUpDisplay.RemoveAllPointers();
        HeadsUpDisplay.AddEvent("player_unattached");
        HeadsUpDisplay.RemoveEvent("attached_damaged_enemy");
        CameraController.CameraEffector.ClearAllEffects();
    }

    public void AttachedCapturedTerritory(string name)
    {
        HeadsUpDisplay.AddEvent("attached_captured_territory");
    }


    public void ShowEmojiMenu()
    {
        if (NetworkSessionManager.IsLocal)
            return;
        if (_lastKillerGamerId == 0)
            return;
        if (GameManager.Instance.LocalProfileCount() != 1)
            return;
        if (!GameManager.Instance.Game.Playing)
            return;

        MultiplayerManager.Instance.EmojiMenuControlGroup.Show(Profile, true);
    }
    public void ShowEmoji(byte type, string fromName)
    {
        HeadsUpDisplay.AddEmoji(type, fromName);
    }
    [MethodReference("send_emoji_angry")]
    public void SendEmojiAngry()
    {
        NetworkSessionNode.Instance.CmdSendEmoji(_lastKillerGamerId, Profile.Name, 0);
    }
    [MethodReference("send_emoji_omg")]
    public void SendEmojiWtf()
    {
        NetworkSessionNode.Instance.CmdSendEmoji(_lastKillerGamerId, Profile.Name, 1);
    }
    [MethodReference("send_emoji_love")]
    public void SendEmojiLove()
    {
        NetworkSessionNode.Instance.CmdSendEmoji(_lastKillerGamerId, Profile.Name, 2);
    }
    [MethodReference("send_emoji_haha")]
    public void SendEmojiHaha()
    {
        NetworkSessionNode.Instance.CmdSendEmoji(_lastKillerGamerId, Profile.Name, 3);
    }
    [MethodReference("send_emoji_tears")]
    public void SendEmojiTears()
    {
        NetworkSessionNode.Instance.CmdSendEmoji(_lastKillerGamerId, Profile.Name, 4);
    }

    private void SetBeacon()
    {
        if (!GameManager.Instance.Game.GameType.TeamGame || Time.time < _beaconCooledDownTime)
            return;

        // Get beacon position
        RaycastHit raycastHit;
        Physics.Raycast(CameraController.transform.position, CameraController.transform.forward, out raycastHit, Mathf.Infinity, ~HUDManager.Instance.BeaconSetIgnoredLayers);

        if (raycastHit.point == Vector3.zero)
            return;

        _beaconCooledDownTime = Time.time + HUDManager.Instance.BeaconCooldownDuration;

        // Instantiate beacon
        GameObject beaconGameObject = Instantiate(HUDManager.Instance.BeaconPrefab, raycastHit.point, Quaternion.identity, Globals.Instance.Containers.UI);
        HeadsUpDisplayBeaconNode beacon = beaconGameObject.GetComponent<HeadsUpDisplayBeaconNode>();
        beacon.Cast(Profile.TeamId);

        // Create network beacon
        if (!NetworkSessionManager.IsLocal)
            NetworkSessionNode.Instance.CmdSpawnBeacon(Profile.TeamId, raycastHit.point);

    }


    /// <summary>
    /// Instantly positions the camera to a respawn location.
    /// </summary>
    [MethodReference("respawn")]
    public void Respawn()
    {
        if (InanimateObject != null)
            InanimateObject.Kill(true);
        CameraController.SetAttributes(false);
        SpawnManager.Instance.RespawnCamera(this);
        XboxInputManager.ClearControllerVibrations(Profile.ControllerId);
        AwaitingRespawn = false;
    }
    /// <summary>
    /// Positions the camera to a respawn location at the delay of the active gametype's respawn time.
    /// </summary>
    [MethodReference("start_delayed_respawn")]
    public void StartDelayedRespawn()
    {
        AwaitingRespawn = true;

        if (InanimateObject != null)
            InanimateObject.Kill(true);

        if (GameManager.Instance.Game.Playing)
            StartCoroutine(DelayRespawn());
    }
    private IEnumerator DelayRespawn()
    {
        // Waits the respawn duration, then respawns the camera controller.
        yield return new WaitForSeconds(GameManager.Instance.Game.GameType.RespawnDuration);
        Respawn();
    }
    #endregion
}
