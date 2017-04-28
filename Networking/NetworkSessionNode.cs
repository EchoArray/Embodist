using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkSessionNode : NetworkBehaviour
{
    public static NetworkSessionNode Instance;

    #region Unity Functions
    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        if (!hasAuthority)
            return;

        Instance = this;

        GameManager.Instance.AllowLocalPlayerJoining = false;
        CmdRequestJoin(GameManager.Instance.Game.Profiles.Count);
    }

    private void Update()
    {
    }
    #endregion

    #region Functions
    #region Game Management

    [Command]
    public void CmdRequestJoin(int localPlayerCount)
    {
        if (GameManager.Instance.Game.Profiles.Count == GameManager.Instance.MaxProfileCount)
        {
            TargetReturnAllowJoin(connectionToClient, false, 0);
            return;
        }
        else if (GameManager.Instance.Game.Profiles.Count + localPlayerCount > 8)
        {
            TargetReturnAllowJoin(connectionToClient, false, 1);
            return;
        }
        else if (GameManager.Instance.Game.Playing)
        {
            TargetReturnAllowJoin(connectionToClient, false, 2);
            return;
        }

        TargetReturnAllowJoin(connectionToClient, true, -1);
        NetworkSessionNode.Instance.RegisterAllProfilesForClient(connectionToClient);
        NetworkSessionNode.Instance.ChangeAllGameSettingsForClient(connectionToClient);
        NetworkSessionNode.Instance.TargetChangeMapForClient(connectionToClient, GameManager.Instance.Game.SelectedMapName);
    }

    [TargetRpc]
    public void TargetReturnAllowJoin(NetworkConnection target, bool allow, short errorCode)
    {
        // Error codes
        // 0 - Session full
        // 1 - No room for players
        // 2 - Game at play

        GameManager.Instance.AllowLocalPlayerJoining = true;

        if (allow)
        {
            RegisterAllProfilesOnHost();
        }
        else
        {
            NetworkSessionManager.Instance.Disconnect();
            switch (errorCode)
            {
                case 0:
                    Debug.Log("This Game Is Full");
                    break;
                case 1:
                    Debug.Log("Your Party Size Is Too Large To Join This Game");
                    break;
                case 2:
                    Debug.Log("This Game Is In Session");
                    break;
            }
        }
    }

    [ClientRpc]
    public void RpcEndGameForClients()
    {
        GameManager.Instance.EndGame();
    }

    public void ChangeAllGameSettingsForClient(NetworkConnection target)
    {
        TargetChangeGameSettingForClient(target, "mode", (int)GameManager.Instance.Game.GameType.Mode);
        TargetChangeGameSettingForClient(target, "score_to_win", (int)GameManager.Instance.Game.GameType.ScoreToWin);
        TargetChangeGameSettingForClient(target, "time_limit", (int)GameManager.Instance.Game.GameType.TimeLimit);
        TargetChangeGameSettingForClient(target, "team_game", GameManager.Instance.Game.GameType.TeamGame ? 1 : 0);
        TargetChangeGameSettingForClient(target, "isolate_areas", GameManager.Instance.Game.GameType.IsolateAreas ? 1 : 0);
        TargetChangeGameSettingForClient(target, "ketchup_only", GameManager.Instance.Game.GameType.KetchupOnly ? 1 : 0);
        TargetChangeGameSettingForClient(target, "throwy_allowed", GameManager.Instance.Game.GameType.ThrowyAllowed ? 1 : 0);
        TargetChangeGameSettingForClient(target, "squirty_allowed", GameManager.Instance.Game.GameType.SquirtyAllowed ? 1 : 0);
        TargetChangeGameSettingForClient(target, "floppy_allowed", GameManager.Instance.Game.GameType.FloppyAllowed ? 1 : 0);

    }
    [TargetRpc]
    public void TargetChangeGameSettingForClient(NetworkConnection target, string setting, int value)
    {
        if (NetworkSessionManager.IsHost)
            return;

        switch (setting)
        {
            // Mode
            case "mode":
                GameManager.Instance.Game.GameType.Mode = (GameManager.GameAspects.GameSettings.GameMode)value;
                break;
            // Score
            case "score_to_win":
                GameManager.Instance.Game.GameType.ScoreToWin = (GameManager.GameAspects.GameSettings.Score)value;
                break;
            // Team Game
            case "team_game":
                GameManager.Instance.Game.GameType.TeamGame = value == 0 ? false : true;
                break;
            // Isolate Areas
            case "isolate_areas":
                GameManager.Instance.Game.GameType.IsolateAreas = value == 0 ? false : true;
                break;
            // Ketchup Only
            case "ketchup_only":
                GameManager.Instance.Game.GameType.KetchupOnly = value == 0 ? false : true;
                break;
            // Throwy Allowed
            case "throwy_allowed":
                GameManager.Instance.Game.GameType.ThrowyAllowed = value == 0 ? false : true;
                break;
            // Squirty Allowed
            case "squirty_allowed":
                GameManager.Instance.Game.GameType.SquirtyAllowed = value == 0 ? false : true;
                break;
            // Floppy Allowed
            case "floppy_allowed":
                GameManager.Instance.Game.GameType.FloppyAllowed = value == 0 ? false : true;
                break;
            case "time_limit":
                GameManager.Instance.Game.GameType.TimeLimit = (GameManager.GameAspects.GameSettings.Duration)value;
                break;
        }
        TitleMenuManager.Instance.CustomGameMenu.LoadAllSettings();
    }
    [ClientRpc]
    public void RpcChangeGameSettingForClients(string setting, int value)
    {
        if (NetworkSessionManager.IsHost)
            return;

        switch (setting)
        {
                // Mode
            case "mode":
                GameManager.Instance.Game.GameType.Mode = (GameManager.GameAspects.GameSettings.GameMode)value;
                break;
                // Score
            case "score_to_win":
                GameManager.Instance.Game.GameType.ScoreToWin = (GameManager.GameAspects.GameSettings.Score)value;
                break;
            // Team Game
            case "team_game":
                GameManager.Instance.Game.GameType.TeamGame = value == 0 ? false : true;
                break;
            // Isolate Areas
            case "isolate_areas":
                GameManager.Instance.Game.GameType.IsolateAreas = value == 0 ? false : true;
                break;
            // Ketchup Only
            case "ketchup_only":
                GameManager.Instance.Game.GameType.KetchupOnly = value == 0 ? false : true;
                break;
            // Throwy Allowed
            case "throwy_allowed":
                GameManager.Instance.Game.GameType.ThrowyAllowed = value == 0 ? false : true;
                break;
            // Squirty Allowed
            case "squirty_allowed":
                GameManager.Instance.Game.GameType.SquirtyAllowed = value == 0 ? false : true;
                break;
            // Floppy Allowed
            case "floppy_allowed":
                GameManager.Instance.Game.GameType.FloppyAllowed = value == 0 ? false : true;
                break;
            case "time_limit":
                GameManager.Instance.Game.GameType.TimeLimit = (GameManager.GameAspects.GameSettings.Duration)value;
                break;
        }
        TitleMenuManager.Instance.CustomGameMenu.LoadAllSettings();
    }
    [ClientRpc]
    public void RpcChangeMapForClients(string sceneName)
    {
        if (NetworkSessionManager.IsHost)
            return;

        MenuMapSelector menuMapSelector = FindObjectOfType<MenuMapSelector>();
        menuMapSelector.ChangeToMap(sceneName);
    }
    [TargetRpc]
    public void TargetChangeMapForClient(NetworkConnection target, string sceneName)
    {
        if (NetworkSessionManager.IsHost)
            return;

        MenuMapSelector menuMapSelector = FindObjectOfType<MenuMapSelector>();
        menuMapSelector.ChangeToMap(sceneName);
    }


    // Client to host
    [Command]
    public void CmdAddFeedItem(string text)
    {
        RpcAddFeedItem(text);
    }
    [ClientRpc]
    public void RpcAddFeedItem(string text)
    {
        HUDManager.Instance.AddFeedItem(text);
    }

    [Command]
    public void CmdAddProfileKill(int gamerId)
    {
        GameManager.Instance.AddProfileKill(gamerId, true);
    }
    [Command]
    public void CmdAddProfileDeath(int gamerId)
    {
        GameManager.Instance.AddProfileDeath(gamerId, true);
    }
    [Command]
    public void CmdAddProfileScore(int gamerId)
    {
        GameManager.Instance.AddProfileScore(gamerId, true);
    }
    // Host to client
    [ClientRpc]
    public void RpcAddProfileKill(int gamerId)
    {
        if (NetworkSessionManager.IsHost)
            return;

        GameManager.Instance.AddProfileKill(gamerId, true);
    }
    [ClientRpc]
    public void RpcAddProfileDeath(int gamerId)
    {
        if (NetworkSessionManager.IsHost)
            return;

        GameManager.Instance.AddProfileDeath(gamerId, true);
    }
    [ClientRpc]
    public void RpcAddProfileScore(int gamerId)
    {
        if (NetworkSessionManager.IsHost)
            return;

        GameManager.Instance.AddProfileScore(gamerId, true);
    }
    #endregion

    #region Player Management
    // Host to client
    [ClientRpc]
    public void RpcPeerDisconnected(int connectionId)
    {
        GameManager.Instance.RemoveAllProfilesForConnection(connectionId);
    }

    // Registration
    public void RegisterAllProfilesForClient(NetworkConnection target)
    {
        foreach (GameManager.GameAspects.Profile profile in GameManager.Instance.Game.Profiles)
            TargetRegisterProfileForClient(target, profile.Name, profile.GamerId, profile.TeamId, profile.ConnectionId);

    }
    [TargetRpc]
    public void TargetRegisterProfileForClient(NetworkConnection target, string name, int gamerId, int teamId, int connectionId)
    {
        if (NetworkSessionManager.IsHost)
            return;
        GameManager.Instance.RegisterNetworkedProfile(name, gamerId, teamId, connectionId);
    }
    [ClientRpc]
    public void RpcRegisterProfileForClients(string name, int gamerId, int teamId, int connectionId)
    {
        if (NetworkSessionManager.IsHost)
            return;
        GameManager.Instance.RegisterNetworkedProfile(name, gamerId, teamId, connectionId);
    }

    // Dismissal
    [ClientRpc]
    public void RpcDismissProfileForClients(int gamerId)
    {
        if (NetworkSessionManager.IsHost)
            return;
        GameManager.Instance.DismissNetworkedProfile(gamerId);
    }

    // Team switching
    [ClientRpc]
    public void RpcSwitchProfileTeamForClients(int gamerId, int teamId)
    {
        if (NetworkSessionManager.IsHost)
            return;
        GameManager.Instance.SwitchNetworkedProfileTeam(gamerId, teamId);
    }


    // Client to Host

    // Registration
    public void RegisterAllProfilesOnHost()
    {
        foreach (GameManager.GameAspects.Profile profile in GameManager.Instance.Game.Profiles)
            CmdRegisterProfileOnHost(profile.Name, profile.GamerId, profile.TeamId);
    }
    [Command]
    public void CmdRegisterProfileOnHost(string name, int gamerId, int teamId)
    {
        GameManager.Instance.RegisterNetworkedProfile(name, gamerId, teamId, connectionToClient.connectionId);
        RpcRegisterProfileForClients(name, gamerId, teamId, connectionToClient.connectionId);
    }

    // Dismissal
    [Command]
    public void CmdDismissProfileOnHost(int gamerId)
    {
        GameManager.Instance.DismissNetworkedProfile(gamerId);
        RpcDismissProfileForClients(gamerId);
    }

    // Team switching
    [Command]
    public void CmdSwitchProfileTeamOnHost(int gamerId, int teamId)
    {
        GameManager.Instance.SwitchNetworkedProfileTeam(gamerId, teamId);
        RpcSwitchProfileTeamForClients(gamerId, teamId);
    }
    #endregion

    #region Requests

    // Attachment
    [Command]
    public void CmdRequestAttach(NetworkIdentity networkIdentity, int localPlayerId, int gamerId, int teamId)
    {
        InanimateObject inanimateObject = networkIdentity.gameObject.GetComponent<InanimateObject>();
        bool allow = false;
        if (!inanimateObject.Controlled)
        {
            allow = true;
            inanimateObject.Controlled = true;
            inanimateObject.NetworkTransform.sendInterval = 0.03243f;
            inanimateObject.NetworkTransform.interpolateMovement = 1;

            inanimateObject.RpcSetControlled(gamerId, teamId);
            networkIdentity.AssignClientAuthority(connectionToClient);
        }
        TargetAttachmentResponse(connectionToClient, localPlayerId, networkIdentity, allow);

    }
    [TargetRpc]
    public void TargetAttachmentResponse(NetworkConnection target, int controlledId, NetworkIdentity networkIdentity, bool allow)
    {
        GameManager.GameAspects.Profile profile = GameManager.Instance.Game.Profiles.Find(p => p.ControllerId == controlledId);
        if (allow)
            profile.LocalPlayer.ForceAttach(networkIdentity.gameObject.GetComponent<InanimateObject>());
        profile.LocalPlayer.CameraController.AwaitingAttachment = false;
    }

    // Powerup
    [Command]
    public void CmdRequestPowerup(GameObject gameObject, GameObject applicant, int type)
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
            TargetApplyPowerup(connectionToClient, applicant, type);
        }
    }
    [TargetRpc]
    public void TargetApplyPowerup(NetworkConnection target, GameObject applicant, int type)
    {
        InanimateObject inanimateObject = applicant.GetComponent<InanimateObject>();
        if (inanimateObject != null)
            inanimateObject.ApplyPowerup((Powerup.PowerupType)type);
    }
    #endregion

    #region Spawning

    // Prefab
    public int GetPrefabIndex(GameObject gameObject)
    {
        return NetworkSessionManager.Instance.PrefabIndexes[gameObject];
    }
    public void SpawnPrefab(GameObject gameObject, Vector3 position, Quaternion rotation)
    {
        int prefabIndex = GetPrefabIndex(gameObject);
        CmdSpawnPrefab(prefabIndex, position, rotation);
    }
    [Command]
    public void CmdSpawnPrefab(int index, Vector3 position, Quaternion rotation)
    {
        GameObject gameObject = Instantiate(NetworkSessionManager.singleton.spawnPrefabs[index], position, rotation);
        NetworkServer.Spawn(gameObject);
    }

    // Projectile
    public void SpawnProjectile(int gamerId, GameObject gameObject, Vector3 position, Quaternion rotation, bool host)
    {
        int prefabIndex = GetPrefabIndex(gameObject);
        CmdSpawnProjectile(gamerId, prefabIndex, position, rotation, host);
    }
    [Command]
    public void CmdSpawnProjectile(int gamerId, int index, Vector3 position, Quaternion rotation, bool host)
    {
        GameObject gameObject = Instantiate(NetworkSessionManager.singleton.spawnPrefabs[index], position, rotation);
        Projectile projectile = gameObject.GetComponent<Projectile>();
        projectile.SetDefaults(gamerId, host);

        NetworkServer.SpawnWithClientAuthority(gameObject, connectionToClient);
    }

    // Effect Utility
    public void SpawnEffectUtility(int gamerId, GameObject gameObject, Vector3 position, Quaternion rotation, bool host)
    {
        int prefabIndex = GetPrefabIndex(gameObject);
        CmdSpawnEffectUtility(gamerId, prefabIndex, position, rotation, host);
    }
    [Command]
    public void CmdSpawnEffectUtility(int gamerId, int index, Vector3 position, Quaternion rotation, bool host)
    {
        GameObject gameObject = Instantiate(NetworkSessionManager.singleton.spawnPrefabs[index], position, rotation);

        EffectUtility effectUtility = gameObject.GetComponent<EffectUtility>();
        if(effectUtility.Type == EffectUtility.ExecutionType.Internal)
            effectUtility.Cast(null, gamerId, host);

        NetworkServer.SpawnWithClientAuthority(gameObject, connectionToClient);
    }

    // Beacons
    [Command]
    public void CmdSpawnBeacon(int teamId, Vector3 position)
    {
        // Send beacon to all clients except the initial commander
        foreach (NetworkConnection connection in NetworkServer.connections)
        {
            if (connection == connectionToClient)
                continue;
            TargetSpawnBeacon(connection, teamId, position);
        }
    }
    [TargetRpc]
    public void TargetSpawnBeacon(NetworkConnection target, int teamId, Vector3 position)
    {
        GameObject gameObject = Instantiate(HUDManager.Instance.BeaconPrefab, position, Quaternion.identity);
        HeadsUpDisplayBeaconNode beacon = gameObject.GetComponent<HeadsUpDisplayBeaconNode>();
        beacon.Cast(teamId);
    }
    #endregion

    // Destroy from client
    [Command]
    public void CmdDestroy(GameObject gameObject)
    {
        if (gameObject != null)
            NetworkServer.Destroy(gameObject);
    }


    // Damage
    [Command]
    public void CmdTransferDamage(int casterId, int affectieId, float damage)
    {
        GameManager.GameAspects.Profile profile = GameManager.Instance.GetProfileByGamerId(affectieId);

        Vector3 position = GameManager.Instance.GetInanimateByGamerId(casterId).transform.position;

        if (profile.Local)
        {
            if (profile.LocalPlayer.InanimateObject != null)
                profile.LocalPlayer.InanimateObject.Damage(damage, position, casterId);
        }
        else
        {
            int connectionId = GameManager.Instance.GetProfileByGamerId(affectieId).ConnectionId;
            TargetTransferDamage(NetworkSessionManager.Instance.GetConnectionById(connectionId), casterId, affectieId, damage);
        }
    }
    [TargetRpc]
    public void TargetTransferDamage(NetworkConnection target, int casterId, int affectieId, float damage)
    {
        GameManager.GameAspects.Profile profile = GameManager.Instance.GetProfileByGamerId(affectieId);

        InanimateObject inanimateObject = GameManager.Instance.GetInanimateByGamerId(casterId);

        Vector3 position = Vector3.zero;
        if (inanimateObject != null)
            position = inanimateObject.transform.position;

        if (profile.LocalPlayer.InanimateObject != null)
            profile.LocalPlayer.InanimateObject.Damage(damage, position, casterId);
    }


    // Emojis
    [Command]
    public void CmdSendEmoji(int gamerId, string fromName, byte type)
    {
        GameManager.GameAspects.Profile profile = GameManager.Instance.GetProfileByGamerId(gamerId);
        if (profile == null)
            return;

        if (profile.Local)
        {
            profile.LocalPlayer.ReceiveEmoji(type, fromName);
        }
        else
        {
            NetworkConnection targetNetworkConnection = NetworkSessionManager.Instance.GetConnectionById(profile.ConnectionId);
            TargetSendEmojiToClient(targetNetworkConnection, gamerId, fromName, type);
        }
    }
    [TargetRpc]
    public void TargetSendEmojiToClient(NetworkConnection target, int gamerId, string fromName, byte type)
    {
        GameManager.GameAspects.Profile profile = GameManager.Instance.GetProfileByGamerId(gamerId);
        if (profile == null)
            return;
        profile.LocalPlayer.ReceiveEmoji(type, fromName);
    }
    #endregion
}
