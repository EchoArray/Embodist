using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    // TODO: Create unity scene editor extension

    #region Values
    public static SpawnManager Instance;
    public GameObject Ketchup;
    public float RespawnDistance;

    [Serializable]
    public class TeamSpawn
    {
        /// <summary>
        /// Defines the name of the team spawn.
        /// </summary>
        public string Name;
        /// <summary>
        /// Defines the position of the team spawn.
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Defines the rotation of the team spawn.
        /// </summary>
        public Vector3 Rotation;
    }
    public List<TeamSpawn> TeamSpawns;

    [Serializable]
    public class PlayerSpawn
    {
        /// <summary>
        /// Defines the name of the player spawn.
        /// </summary>
        public string Name;
        /// <summary>
        /// Defines the position of the player spawn.
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Defines the rotation of the player spawn.
        /// </summary>
        public Vector3 Rotation;
    }
    public List<PlayerSpawn> PlayerSpawns;

    [Serializable]
    public class ObjectiveSpawn
    {
        /// <summary>
        /// Defines the name of the objective spawn.
        /// </summary>
        public string Name;
        /// <summary>
        /// Defines the mode in-which the objective spawn will be spawned in.
        /// </summary>
        public GameManager.GameAspects.GameSettings.GameMode Mode;
        /// <summary>
        /// Defines the team id of the objective spawn.
        /// </summary>
        public int TeamId;
        /// <summary>
        /// Defines the position of the objective spawn.
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Defines the offset applied to the Y axis of the position when hot lava is active.
        /// </summary>
        public float HotLazaYOffset;
        /// <summary>
        /// Defines the rotation of the objective spawn along the y-axis.
        /// </summary>
        public float YRotation;
    }
    public List<ObjectiveSpawn> ObjectiveSpawns;
    
    [Serializable]
    public class ObjectSpawn
    {
        /// <summary>
        /// Defines the name of the spawn.
        /// </summary>
        [Space(15)]
        public string Name;

        /// <summary>
        /// Defines the team id of the spawn.
        /// </summary>
        public int TeamId;

        /// <summary>
        /// Determines if the spawn is ignored this game.
        /// </summary>
        [HideInInspector]
        public bool Ignore;
        /// <summary>
        /// Determines if the spawn isn't spawned as a ketchup while ketchup only is enabled.
        /// </summary>
        public bool IgnoreKetchupOnly;
        /// <summary>
        /// Defines the object to be spawned.
        /// </summary>
        public GameObject ObjectPrefab;
        /// <summary>
        /// Defines the active object.
        /// </summary>
        [HideInInspector]
        public GameObject ActiveObject;
        /// <summary>
        /// Determines if the spawn has yet to be spawned.
        /// </summary>
        [HideInInspector]
        public bool InitialSpawn = true;
        /// <summary>
        /// Determines if the spawn is waiting to be re-spawned.
        /// </summary>
        [HideInInspector]
        public bool AwaitingRespawn;
        /// <summary>
        /// Defines the delay at which the spawn will wait to respawn at.
        /// </summary>
        public float RespawnDelay;
        /// <summary>
        /// Defines the time in-which the spawn will respawn.
        /// </summary>
        [HideInInspector]
        public float RespawnTime;

        /// <summary>
        /// Defines the position of the spawn.
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Defines the rotation of the spawn.
        /// </summary>
        public Vector3 Rotation;
        /// <summary>
        /// Defines the child indexes of the spawn.
        /// </summary>
        public int[] CoRespawnedChildIndexes;

        public ObjectSpawn(ObjectSpawn objectSpawn)
        {
            IgnoreKetchupOnly = objectSpawn.IgnoreKetchupOnly;
            ObjectPrefab = objectSpawn.ObjectPrefab;
            ActiveObject = objectSpawn.ActiveObject;
            RespawnDelay = objectSpawn.RespawnDelay;
            Position = objectSpawn.Position;
            Rotation = objectSpawn.Rotation;
            InitialSpawn = true;
        }
    }
    public List<ObjectSpawn> ObjectSpawns;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        ClearAllSpawns();
        Initialize();
    }
    private void Start()
    {
        SpawnObjectives();
    }

    private void Update()
    {
        UpdateObjectRespawns();
    }

    private void OnDrawGizmos()
    {
        // Draw spawn locations
        foreach (PlayerSpawn playerSpawn in PlayerSpawns)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerSpawn.Position, 0.5f);
            Gizmos.DrawLine(playerSpawn.Position, playerSpawn.Position + (Quaternion.Euler(playerSpawn.Rotation) * Vector3.forward) * 1);

            Gizmos.DrawWireSphere(playerSpawn.Position + (Quaternion.Euler(playerSpawn.Rotation) * Vector3.forward) * 1, 0.15f);
        }
        foreach (TeamSpawn teamSpawn in TeamSpawns)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(teamSpawn.Position, 0.5f);
            Gizmos.DrawLine(teamSpawn.Position, teamSpawn.Position + (Quaternion.Euler(teamSpawn.Rotation) * Vector3.forward) * 1);

            Gizmos.DrawWireSphere(teamSpawn.Position + (Quaternion.Euler(teamSpawn.Rotation) * Vector3.forward) * 1, 0.15f);
        }
        foreach (ObjectiveSpawn objectiveSpawn in ObjectiveSpawns)
        {
            Matrix4x4 matrixBackup = Gizmos.matrix;

            Gizmos.matrix = Matrix4x4.TRS(objectiveSpawn.Position, Quaternion.Euler(0, objectiveSpawn.YRotation, 0), Vector3.one);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(Vector3.zero, Vector3.forward);
            Gizmos.DrawWireSphere(Vector3.zero, 2f);


            Vector3 hotLavaOffset = new Vector3(0, objectiveSpawn.HotLazaYOffset,0);

            Gizmos.DrawLine(Vector3.zero, hotLavaOffset);

            Gizmos.color = new Color(1, 1, 0, 0.5f);
            Gizmos.DrawWireSphere(hotLavaOffset, 2f);
            Gizmos.matrix = matrixBackup;
        }
        foreach (ObjectSpawn objectSpawn in ObjectSpawns)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(objectSpawn.Position, 0.25f);
        }
    } 
    #endregion

    #region Functions
    private void Initialize()
    {
        Instance = this;
        GetIgnoredSpawns();
    }

    private float _nextCheckDistanceTime;
    private void UpdateObjectRespawns()
    {
        if (NetworkSessionManager.IsClient)
            return;
        bool checkDistance = _nextCheckDistanceTime < Time.time;
        foreach (ObjectSpawn objectSpawn in ObjectSpawns)
        {
            if (objectSpawn.Ignore)
                continue;

            if (objectSpawn.ActiveObject != null && checkDistance)
            {
                // If the object is spawned, not controlled, and too far from its original spawn point, respawn
                InanimateObject inanimateObject = objectSpawn.ActiveObject.GetComponent<InanimateObject>();
                if (inanimateObject != null && !inanimateObject.Controlled)
                {
                    float distanceFromSpawn = Vector3.Distance(objectSpawn.Position, objectSpawn.ActiveObject.transform.position);
                    if (distanceFromSpawn > RespawnDistance)
                    {
                        ResetObject(objectSpawn);
                        ResetChildObjects(objectSpawn);
                    }
                }
            }
            else if (!objectSpawn.InitialSpawn && objectSpawn.ActiveObject == null && !objectSpawn.AwaitingRespawn)
            {
                // If it isn't the initial spawn, there is no active object and the index isn't awaiting respawn
                // Set respawn time and define awaiting
                objectSpawn.RespawnTime = Time.time + objectSpawn.RespawnDelay;
                objectSpawn.AwaitingRespawn = true;
            }
            else if (objectSpawn.InitialSpawn || (objectSpawn.AwaitingRespawn && Time.time >= objectSpawn.RespawnTime))
            {
                // Otherwise, if the object has yet to be spawned or the object is awaiting spawn and the respawn time is up
                RespawnObject(objectSpawn);
            }
        }
        if (checkDistance)
            _nextCheckDistanceTime = Time.time + 1;
    }

    private void GetIgnoredSpawns()
    {
        foreach (ObjectSpawn objectSpawn in ObjectSpawns)
        {
            InanimateObject prefabInanimateObject = objectSpawn.ObjectPrefab.GetComponent<InanimateObject>();
            if (prefabInanimateObject != null && prefabInanimateObject.Class == InanimateObject.Classification.Bomb && GameManager.Instance.Game.GameType.Mode != GameManager.GameAspects.GameSettings.GameMode.Bomb)
                objectSpawn.Ignore = true;
        }
    }

    public void RespawnObject(ObjectSpawn objectSpawn)
    {
        // Spawn the object and define the instances values
        GameObject spawnedObject = GameManager.Instance.Game.GameType.KetchupOnly && !objectSpawn.IgnoreKetchupOnly ? Ketchup : objectSpawn.ObjectPrefab;
        GameObject newObject = Instantiate(spawnedObject, objectSpawn.Position, Quaternion.Euler(objectSpawn.Rotation));


        if (NetworkSessionManager.IsHost)
            NetworkServer.Spawn(newObject);

        newObject.name = objectSpawn.ObjectPrefab.name;

        InanimateObject inanimateObject = newObject.GetComponent<InanimateObject>();
        if (inanimateObject != null)
            inanimateObject.TeamId = objectSpawn.TeamId;

        objectSpawn.ActiveObject = newObject;
        objectSpawn.InitialSpawn = false;
        objectSpawn.AwaitingRespawn = false;

        ResetChildObjects(objectSpawn);
    }
    private void ResetObject(ObjectSpawn objectSpawn)
    {
        if (objectSpawn.ActiveObject != null)
        {
            InanimateObject inanimateObject = objectSpawn.ActiveObject.GetComponent<InanimateObject>();
            if (inanimateObject != null && !inanimateObject.Controlled)
            {
                // Reset velocities
                inanimateObject.ResetVelocity();

                // Reset position and rotation
                objectSpawn.ActiveObject.transform.position = objectSpawn.Position;
                objectSpawn.ActiveObject.transform.eulerAngles = objectSpawn.Rotation;
            }
        }
    }
    private void ResetChildObjects(ObjectSpawn objectSpawn)
    {
        for (int i = 0; i < objectSpawn.CoRespawnedChildIndexes.Length; i++)
        {
            ObjectSpawn child = ObjectSpawns[objectSpawn.CoRespawnedChildIndexes[i]];
            ResetObject(child);
        }
    }

    public void ClearAllSpawns()
    {
        foreach (ObjectSpawn objectSpawn in ObjectSpawns)
        {
            objectSpawn.InitialSpawn = true;
            DestroyImmediate(objectSpawn.ActiveObject);
        }
    }
    public void RespawnLocalCamera(LocalPlayer localPlayer)
    {
        // Reset camera position to a spawn point
        if (GameManager.Instance.Game.GameType.TeamGame)
        {
            // If it is a team game, find the players team spawn location
            localPlayer.CameraController.transform.position = SpawnManager.Instance.TeamSpawns[localPlayer.Profile.TeamId].Position;
            localPlayer.CameraController.transform.eulerAngles = SpawnManager.Instance.TeamSpawns[localPlayer.Profile.TeamId].Rotation;
        }
        else
        {
            // Otherwise randomize the spawn location among the available positions
            int spawn = UnityEngine.Random.Range(0, SpawnManager.Instance.PlayerSpawns.Count);
            localPlayer.CameraController.transform.position = SpawnManager.Instance.PlayerSpawns[spawn].Position;
            localPlayer.CameraController.transform.eulerAngles = SpawnManager.Instance.PlayerSpawns[spawn].Rotation;
        }
    }

    private void SpawnObjectives()
    {
        if (NetworkSessionManager.IsClient)
            return;

        foreach (SpawnManager.ObjectiveSpawn objective in SpawnManager.Instance.ObjectiveSpawns)
        {
            // If the spawns mode isn't of the current game mode, continue
            if (objective.Mode != GameManager.Instance.Game.GameType.Mode)
                continue;

            // Determine type
            Territory.TerritoryType type = Territory.TerritoryType.Arm;
            switch (objective.Mode)
            {
                case GameManager.GameAspects.GameSettings.GameMode.Capture:
                    type = Territory.TerritoryType.Capture;
                    break;
                case GameManager.GameAspects.GameSettings.GameMode.KingOfTheHill:
                    type = Territory.TerritoryType.Hill;
                    break;
                case GameManager.GameAspects.GameSettings.GameMode.Bomb:
                    type = Territory.TerritoryType.Arm;
                    break;
            }

            // Instantiate and define defaults
            Vector3 position = objective.Position;
            position.y += GameManager.Instance.Game.GameType.HotLava ? objective.HotLazaYOffset : 0;

            GameObject newTerritory = Instantiate(MultiplayerManager.Instance.TerritoryPrefab, position, Quaternion.Euler(0, objective.YRotation, 0), Globals.Instance.Containers.Objectives);
            Territory territory = newTerritory.GetComponent<Territory>();
            territory.SetDefaults(type, objective.TeamId, objective.Name);



            if (NetworkSessionManager.IsHost)
                NetworkServer.Spawn(newTerritory);
        }
    }
    #endregion
}
