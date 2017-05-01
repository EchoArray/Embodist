using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSessionManager : NetworkManager
{
    public static NetworkSessionManager Instance;

    public List<GameObject> InanimateObjects;
    public List<GameObject> Projectiles;
    public List<GameObject> Effects;
    public List<GameObject> PhysicalEffects;
    public List<GameObject> Objectives;

    public Dictionary<GameObject, int> PrefabIndexes = new Dictionary<GameObject, int>();

    public static bool IsClient
    {
        get { return !NetworkServer.active && NetworkClient.active; }
    }
    public static bool IsHost
    {
        get { return NetworkServer.active && NetworkClient.active; }
    }
    public static bool IsLocal
    {
        get { return !NetworkServer.active && !NetworkClient.active; }
    }

    private void Awake()
    {
        if (singleton != null && singleton != this)
        {
            Destroy(this.gameObject);
            return;
        }
        singleton = this;
        Instance = this;
        
        singleton.spawnPrefabs.AddRange(InanimateObjects);
        singleton.spawnPrefabs.AddRange(Projectiles);
        singleton.spawnPrefabs.AddRange(Effects);
        singleton.spawnPrefabs.AddRange(PhysicalEffects);
        singleton.spawnPrefabs.AddRange(Objectives);

        for (int i = 0; i < spawnPrefabs.Count; i++)
            PrefabIndexes.Add(spawnPrefabs[i], i);

        DontDestroyOnLoad(this.gameObject);
    }


    public void StartHost()
    {
        NetworkManager.singleton.StartHost();
    }
    public void Connect(string ip)
    {
        singleton.networkAddress = ip;
        NetworkManager.singleton.StartClient();
    }
    public void Disconnect()
    {
        if (IsHost)
            NetworkManager.singleton.StopHost();
        else if (IsClient)
            NetworkManager.singleton.StopClient();
    }


    public override void OnStartServer()
    {
        base.OnStartServer();
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
    }
    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        base.OnServerAddPlayer(conn, playerControllerId);
    }
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
    }
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);

        GameManager.Instance.RemoveAllProfilesForConnection(conn.connectionId);
        NetworkSessionNode.Instance.RpcPeerDisconnected(conn.connectionId);

        if (GameManager.Instance.Game.Playing && !GameManager.Instance.ProfilesOpposing())
            GameManager.Instance.EndGame();
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        GameManager.Instance.RemoveAllNonLocalProfiles();
        if (GameManager.Instance.Game.Playing && !GameManager.Instance.ProfilesOpposing())
            GameManager.Instance.EndGame();
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        if(TitleMenuManager.Instance != null)
            TitleMenuManager.Instance.ClientStateChanged();
    }
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        GameManager.Instance.AllowLocalPlayerJoining = true;

        GameManager.Instance.RemoveAllNonLocalProfiles();

        if (GameManager.Instance.Game.Playing)
            GameManager.Instance.EndGame();

        if (TitleMenuManager.Instance != null)
            TitleMenuManager.Instance.ClientStateChanged();
    }
    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        base.OnClientError(conn, errorCode);
    }
    public override void OnDropConnection(bool success, string extendedInfo)
    {
        base.OnDropConnection(success, extendedInfo);

        GameManager.Instance.AllowLocalPlayerJoining = true;

        GameManager.Instance.RemoveAllNonLocalProfiles();
        if (GameManager.Instance.Game.Playing)
            GameManager.Instance.EndGame();
    }

    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
    }

    public NetworkConnection GetConnectionById(int connectionId)
    {
        foreach (NetworkConnection networkConnection in NetworkServer.connections)
            if (networkConnection.connectionId == connectionId)
                return networkConnection;

        return null;
    }
}
